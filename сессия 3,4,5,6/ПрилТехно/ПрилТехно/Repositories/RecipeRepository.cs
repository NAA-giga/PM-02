using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using ПрилТехно.Models;
using ПрилТехно.Services;

namespace ПрилТехно.Repositories
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public RecipeRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private IDbConnection CreateConnection() => _connectionFactory.CreateConnection();

        public async Task<List<RecipeDto>> GetAllRecipesAsync()
        {
            const string sql = @"
                SELECT 
                    r.id AS Id,
                    r.product_id AS ProductId,
                    p.name AS ProductName,
                    r.version AS Version,
                    r.name AS Name,
                    r.status AS Status,
                    r.approved_at AS ApprovedAt,
                    r.approved_by AS ApprovedBy,
                    r.created_by AS CreatedBy,
                    r.created_at AS CreatedAt,
                    r.updated_at AS UpdatedAt
                FROM recipes r
                JOIN products p ON r.product_id = p.id
                ORDER BY r.created_at DESC";
            using var conn = CreateConnection();
            return (await conn.QueryAsync<RecipeDto>(sql)).ToList();
        }

        public async Task<RecipeDto?> GetRecipeByIdAsync(int id)
        {
            const string recipeSql = @"
                SELECT 
                    r.id AS Id,
                    r.product_id AS ProductId,
                    p.name AS ProductName,
                    r.version AS Version,
                    r.name AS Name,
                    r.status AS Status,
                    r.approved_at AS ApprovedAt,
                    r.approved_by AS ApprovedBy,
                    r.created_by AS CreatedBy,
                    r.created_at AS CreatedAt,
                    r.updated_at AS UpdatedAt
                FROM recipes r
                JOIN products p ON r.product_id = p.id
                WHERE r.id = @Id";
            using var conn = CreateConnection();
            var recipe = await conn.QueryFirstOrDefaultAsync<RecipeDto>(recipeSql, new { Id = id });
            if (recipe == null) return null;

            const string componentsSql = @"
                SELECT 
                    rc.id AS Id,
                    rc.recipe_id AS RecipeId,
                    rc.raw_material_id AS RawMaterialId,
                    rm.name AS RawMaterialName,
                    rc.percentage AS Percentage,
                    rc.load_order AS LoadOrder,
                    rc.tolerance_min AS ToleranceMin,
                    rc.tolerance_max AS ToleranceMax
                FROM recipe_components rc
                JOIN raw_materials rm ON rc.raw_material_id = rm.id
                WHERE rc.recipe_id = @Id
                ORDER BY rc.load_order";
            recipe.Components = (await conn.QueryAsync<RecipeComponentDto>(componentsSql, new { Id = id })).ToList();
            return recipe;
        }

        public async Task<int> CreateRecipeAsync(RecipeDto recipe)
        {
            using var conn = CreateConnection();
            using var trans = conn.BeginTransaction();
            try
            {
                // Вставка рецептуры
                const string insertRecipe = @"
                    INSERT INTO recipes (product_id, version, name, status, created_by, created_at, updated_at)
                    VALUES (@ProductId, @Version, @Name, 'draft', @CreatedBy, GETDATE(), GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS INT)";
                var recipeId = await conn.ExecuteScalarAsync<int>(insertRecipe, new
                {
                    recipe.ProductId,
                    recipe.Version,
                    recipe.Name,
                    CreatedBy = recipe.CreatedBy // нужно передать текущего пользователя
                }, trans);

                // Вставка компонентов
                const string insertComponent = @"
                    INSERT INTO recipe_components (recipe_id, raw_material_id, percentage, load_order, tolerance_min, tolerance_max, created_at)
                    VALUES (@RecipeId, @RawMaterialId, @Percentage, @LoadOrder, @ToleranceMin, @ToleranceMax, GETDATE())";
                foreach (var comp in recipe.Components)
                {
                    await conn.ExecuteAsync(insertComponent, new
                    {
                        RecipeId = recipeId,
                        comp.RawMaterialId,
                        comp.Percentage,
                        comp.LoadOrder,
                        comp.ToleranceMin,
                        comp.ToleranceMax
                    }, trans);
                }
                trans.Commit();
                return recipeId;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateRecipeAsync(RecipeDto recipe)
        {
            using var conn = CreateConnection();
            using var trans = conn.BeginTransaction();
            try
            {
                // Обновление основной информации
                const string updateRecipe = @"
                    UPDATE recipes 
                    SET name = @Name, updated_at = GETDATE()
                    WHERE id = @Id AND status = 'draft'";
                var rows = await conn.ExecuteAsync(updateRecipe, new { recipe.Id, recipe.Name }, trans);
                if (rows == 0) return false;

                // Удалить старые компоненты
                await conn.ExecuteAsync("DELETE FROM recipe_components WHERE recipe_id = @Id", new { recipe.Id }, trans);
                // Вставить новые
                const string insertComponent = @"
                    INSERT INTO recipe_components (recipe_id, raw_material_id, percentage, load_order, tolerance_min, tolerance_max, created_at)
                    VALUES (@RecipeId, @RawMaterialId, @Percentage, @LoadOrder, @ToleranceMin, @ToleranceMax, GETDATE())";
                foreach (var comp in recipe.Components)
                {
                    await conn.ExecuteAsync(insertComponent, new
                    {
                        RecipeId = recipe.Id,
                        comp.RawMaterialId,
                        comp.Percentage,
                        comp.LoadOrder,
                        comp.ToleranceMin,
                        comp.ToleranceMax
                    }, trans);
                }
                trans.Commit();
                return true;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public async Task<bool> ApproveRecipeAsync(int recipeId, int userId)
        {
            using var conn = CreateConnection();
            using var trans = conn.BeginTransaction();
            try
            {
                // Проверка суммы компонентов = 100
                var total = await conn.ExecuteScalarAsync<decimal>(
                    "SELECT ISNULL(SUM(percentage), 0) FROM recipe_components WHERE recipe_id = @RecipeId", new { RecipeId = recipeId }, trans);
                if (Math.Abs(total - 100) > 0.01m)
                    throw new InvalidOperationException($"Сумма компонентов должна быть 100%, сейчас {total}%");

                // Проверка, что нет другой утверждённой рецептуры для того же продукта
                var productId = await conn.ExecuteScalarAsync<int>(
                    "SELECT product_id FROM recipes WHERE id = @Id", new { Id = recipeId }, trans);
                var approvedExists = await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM recipes WHERE product_id = @ProductId AND status = 'approved' AND id != @Id",
                    new { ProductId = productId, Id = recipeId }, trans) > 0;
                if (approvedExists)
                    throw new InvalidOperationException("Для данного продукта уже существует утверждённая рецептура");

                const string approveSql = @"
                    UPDATE recipes 
                    SET status = 'approved', approved_at = GETDATE(), approved_by = @UserId, updated_at = GETDATE()
                    WHERE id = @Id AND status = 'draft'";
                var rows = await conn.ExecuteAsync(approveSql, new { Id = recipeId, UserId = userId }, trans);
                trans.Commit();
                return rows > 0;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public async Task<bool> ArchiveRecipeAsync(int recipeId)
        {
            using var conn = CreateConnection();
            const string sql = "UPDATE recipes SET status = 'archived', updated_at = GETDATE() WHERE id = @Id AND status IN ('draft', 'approved')";
            var rows = await conn.ExecuteAsync(sql, new { Id = recipeId });
            return rows > 0;
        }

        public async Task<bool> DeleteComponentAsync(int componentId)
        {
            using var conn = CreateConnection();
            const string sql = "DELETE FROM recipe_components WHERE id = @Id";
            var rows = await conn.ExecuteAsync(sql, new { Id = componentId });
            return rows > 0;
        }
    }
}