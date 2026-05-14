using Dapper;
using API.Models.Entities;
using API.Models.DTOs;
using API.Repositories.Interfaces;

namespace API.Repositories
{
    public class RecipeRepository : BaseRepository, IRecipeRepository
    {
        public RecipeRepository(IConfiguration config) : base(config) { }

        public async Task<IEnumerable<Recipe>> GetAllAsync(int? productId = null, string? status = null)
        {
            using var conn = CreateConnection();
            var sql = @"
                SELECT 
                    r.id AS Id,
                    r.product_id AS ProductId,
                    r.version AS Version,
                    r.name AS Name,
                    r.status AS Status,
                    r.approved_at AS ApprovedAt,
                    r.approved_by AS ApprovedBy,
                    r.created_by AS CreatedBy,
                    r.created_at AS CreatedAt,
                    r.updated_at AS UpdatedAt
                FROM recipes r
                WHERE 1=1";
            if (productId.HasValue)
                sql += " AND r.product_id = @ProductId";
            if (!string.IsNullOrEmpty(status))
                sql += " AND r.status = @Status";
            sql += " ORDER BY r.created_at DESC";
            return await conn.QueryAsync<Recipe>(sql, new { ProductId = productId, Status = status });
        }

        public async Task<Recipe?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            const string sql = "SELECT * FROM recipes WHERE id = @Id";
            return await conn.QueryFirstOrDefaultAsync<Recipe>(sql, new { Id = id });
        }

        public async Task<RecipeResponseDto?> GetRecipeDetailsAsync(int id)
        {
            using var conn = CreateConnection();
            const string recipeSql = @"
        SELECT 
            r.id, r.product_id AS ProductId, r.version, r.name, r.status,
            r.approved_at AS ApprovedAt, r.approved_by AS ApprovedBy, 
            r.created_by AS CreatedBy, r.created_at AS CreatedAt, r.updated_at AS UpdatedAt,
            p.name AS ProductName,
            u1.full_name AS ApprovedByName,
            u2.full_name AS CreatedByName
        FROM recipes r
        JOIN products p ON r.product_id = p.id
        LEFT JOIN users u1 ON r.approved_by = u1.id
        JOIN users u2 ON r.created_by = u2.id
        WHERE r.id = @Id";
            var recipe = await conn.QueryFirstOrDefaultAsync<RecipeResponseDto>(recipeSql, new { Id = id });
            if (recipe == null) return null;

            const string compSql = @"
        SELECT 
            rc.id, rc.recipe_id AS RecipeId, rc.raw_material_id AS RawMaterialId,
            rc.percentage, rc.load_order AS LoadOrder, 
            rc.tolerance_min AS ToleranceMin, rc.tolerance_max AS ToleranceMax,
            rm.name AS RawMaterialName
        FROM recipe_components rc
        JOIN raw_materials rm ON rc.raw_material_id = rm.id
        WHERE rc.recipe_id = @Id
        ORDER BY rc.load_order";
            // Используем QueryAsync<RecipeComponentResponseDto> для соответствия типу свойства
            var components = await conn.QueryAsync<RecipeComponentResponseDto>(compSql, new { Id = id });
            recipe.Components = components.ToList();
            return recipe;
        }

        public async Task<int> CreateAsync(Recipe recipe)
        {
            using var conn = CreateConnection();
            const string sql = @"
                INSERT INTO recipes (product_id, version, name, status, created_by, created_at, updated_at)
                VALUES (@ProductId, @Version, @Name, @Status, @CreatedBy, @CreatedAt, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            return await conn.ExecuteScalarAsync<int>(sql, recipe);
        }

        public async Task<bool> UpdateAsync(Recipe recipe)
        {
            using var conn = CreateConnection();
            const string sql = @"
                UPDATE recipes 
                SET name = @Name, version = @Version, status = @Status, updated_at = @UpdatedAt
                WHERE id = @Id";
            var rows = await conn.ExecuteAsync(sql, recipe);
            return rows > 0;
        }

        public async Task<bool> UpdateStatusAsync(int id, string newStatus, int userId)
        {
            using var conn = CreateConnection();
            const string sql = "UPDATE recipes SET status = @Status, updated_at = GETDATE() WHERE id = @Id";
            var rows = await conn.ExecuteAsync(sql, new { Id = id, Status = newStatus });
            return rows > 0;
        }

        public async Task<bool> ApproveAsync(int id, int userId)
        {
            var total = await GetTotalPercentageAsync(id);
            if (Math.Abs(total - 100) > 0.01m)
                throw new InvalidOperationException($"Сумма компонентов должна быть 100%, сейчас {total}%");

            var recipe = await GetByIdAsync(id);
            if (recipe == null) return false;
            if (await IsAnyApprovedForProductAsync(recipe.ProductId, id))
                throw new InvalidOperationException("Для этого продукта уже есть утверждённая рецептура");

            using var conn = CreateConnection();
            const string sql = @"
                UPDATE recipes 
                SET status = 'approved', approved_at = GETDATE(), approved_by = @UserId, updated_at = GETDATE()
                WHERE id = @Id";
            var rows = await conn.ExecuteAsync(sql, new { Id = id, UserId = userId });
            return rows > 0;
        }

        public async Task<bool> ArchiveAsync(int id, int userId)
        {
            using var conn = CreateConnection();
            const string sql = "UPDATE recipes SET status = 'archived', updated_at = GETDATE() WHERE id = @Id";
            var rows = await conn.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }

        public async Task<bool> AddComponentAsync(int recipeId, RecipeComponent component)
        {
            using var conn = CreateConnection();
            const string sql = @"
                INSERT INTO recipe_components (recipe_id, raw_material_id, percentage, load_order, tolerance_min, tolerance_max)
                VALUES (@RecipeId, @RawMaterialId, @Percentage, @LoadOrder, @ToleranceMin, @ToleranceMax)";
            var rows = await conn.ExecuteAsync(sql, new
            {
                RecipeId = recipeId,
                component.RawMaterialId,
                component.Percentage,
                component.LoadOrder,
                component.ToleranceMin,
                component.ToleranceMax
            });
            return rows > 0;
        }

        public async Task<bool> UpdateComponentAsync(RecipeComponent component)
        {
            using var conn = CreateConnection();
            const string sql = @"
                UPDATE recipe_components 
                SET raw_material_id = @RawMaterialId, percentage = @Percentage, load_order = @LoadOrder,
                    tolerance_min = @ToleranceMin, tolerance_max = @ToleranceMax
                WHERE id = @Id";
            var rows = await conn.ExecuteAsync(sql, component);
            return rows > 0;
        }

        public async Task<bool> RemoveComponentAsync(int componentId)
        {
            using var conn = CreateConnection();
            const string sql = "DELETE FROM recipe_components WHERE id = @Id";
            var rows = await conn.ExecuteAsync(sql, new { Id = componentId });
            return rows > 0;
        }

        public async Task<decimal> GetTotalPercentageAsync(int recipeId)
        {
            using var conn = CreateConnection();
            const string sql = "SELECT ISNULL(SUM(percentage), 0) FROM recipe_components WHERE recipe_id = @RecipeId";
            return await conn.ExecuteScalarAsync<decimal>(sql, new { RecipeId = recipeId });
        }

        public async Task<bool> IsAnyApprovedForProductAsync(int productId, int excludeRecipeId = 0)
        {
            using var conn = CreateConnection();
            const string sql = @"
                SELECT COUNT(1) FROM recipes 
                WHERE product_id = @ProductId AND status = 'approved' AND id != @ExcludeId";
            var count = await conn.ExecuteScalarAsync<int>(sql, new { ProductId = productId, ExcludeId = excludeRecipeId });
            return count > 0;
        }
        public async Task<int> GetMaxVersionForProductAsync(int productId)
        {
            using var conn = CreateConnection();
            const string sql = "SELECT ISNULL(MAX(version), 0) FROM recipes WHERE product_id = @ProductId";
            return await conn.ExecuteScalarAsync<int>(sql, new { ProductId = productId });
        }
    }
}