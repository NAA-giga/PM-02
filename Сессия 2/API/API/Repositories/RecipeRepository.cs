using Dapper;
using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories;

public class RecipeRepository : BaseRepository, IRecipeRepository
{
    public RecipeRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<Recipe>> GetAllAsync()
    {
        using var conn = CreateConnection();
        const string sql = @"
            SELECT r.*, p.Name as ProductName 
            FROM Recipes r
            JOIN Products p ON r.ProductId = p.Id
            ORDER BY r.CreatedAt DESC";
        return await conn.QueryAsync<Recipe>(sql);
    }

    public async Task<Recipe?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM Recipes WHERE Id = @Id";
        return await conn.QueryFirstOrDefaultAsync<Recipe>(sql, new { Id = id });
    }

    public async Task<RecipeResponseDto?> GetRecipeDetailsAsync(int id)
    {
        using var conn = CreateConnection();
        const string sql = @"
            SELECT 
                r.Id, r.ProductId, r.Version, r.Name, r.Status, r.ApprovedAt, r.ApprovedBy, 
                r.CreatedBy, r.CreatedAt, r.UpdatedAt,
                p.Name as ProductName,
                u1.FullName as ApprovedByName,
                u2.FullName as CreatedByName
            FROM Recipes r
            JOIN Products p ON r.ProductId = p.Id
            LEFT JOIN Users u1 ON r.ApprovedBy = u1.Id
            JOIN Users u2 ON r.CreatedBy = u2.Id
            WHERE r.Id = @Id";

        var recipe = await conn.QueryFirstOrDefaultAsync<RecipeResponseDto>(sql, new { Id = id });
        if (recipe == null) return null;

        const string componentsSql = @"
            SELECT 
                rc.Id, rc.RawMaterialId, rc.Percentage, rc.LoadOrder, rc.ToleranceMin, rc.ToleranceMax,
                rm.Name as RawMaterialName, rm.Code as RawMaterialCode
            FROM RecipeComponents rc
            JOIN RawMaterials rm ON rc.RawMaterialId = rm.Id
            WHERE rc.RecipeId = @Id
            ORDER BY rc.LoadOrder";

        var components = await conn.QueryAsync<RecipeComponentResponseDto>(componentsSql, new { Id = id });
        recipe.Components = components.ToList();
        return recipe;
    }

    public async Task<int> CreateAsync(CreateRecipeDto dto, int userId)
    {
        using var conn = CreateConnection();
        using var trans = conn.BeginTransaction();
        try
        {
            const string recipeSql = @"
                INSERT INTO Recipes (ProductId, Version, Name, Status, CreatedBy, CreatedAt, UpdatedAt)
                VALUES (@ProductId, @Version, @Name, 'draft', @CreatedBy, GETDATE(), GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            var recipeId = await conn.ExecuteScalarAsync<int>(recipeSql, new
            {
                dto.ProductId,
                dto.Version,
                dto.Name,
                CreatedBy = userId
            }, trans);

            foreach (var comp in dto.Components)
            {
                const string compSql = @"
                    INSERT INTO RecipeComponents (RecipeId, RawMaterialId, Percentage, LoadOrder, ToleranceMin, ToleranceMax)
                    VALUES (@RecipeId, @RawMaterialId, @Percentage, @LoadOrder, @ToleranceMin, @ToleranceMax)";
                await conn.ExecuteAsync(compSql, new
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

    public async Task<bool> UpdateAsync(int id, UpdateRecipeDto dto, int userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
            UPDATE Recipes 
            SET Name = @Name, UpdatedAt = GETDATE()
            WHERE Id = @Id AND Status != 'approved'";
        var rows = await conn.ExecuteAsync(sql, new { dto.Name, Id = id });
        if (rows == 0) return false;

        // Обновление компонентов (удаление старых и вставка новых)
        const string deleteComponents = "DELETE FROM RecipeComponents WHERE RecipeId = @Id";
        await conn.ExecuteAsync(deleteComponents, new { Id = id });
        foreach (var comp in dto.Components)
        {
            const string insertComp = @"
                INSERT INTO RecipeComponents (RecipeId, RawMaterialId, Percentage, LoadOrder, ToleranceMin, ToleranceMax)
                VALUES (@RecipeId, @RawMaterialId, @Percentage, @LoadOrder, @ToleranceMin, @ToleranceMax)";
            await conn.ExecuteAsync(insertComp, new
            {
                RecipeId = id,
                comp.RawMaterialId,
                comp.Percentage,
                comp.LoadOrder,
                comp.ToleranceMin,
                comp.ToleranceMax
            });
        }
        return true;
    }

    public async Task<bool> AddComponentAsync(int recipeId, RecipeComponentDto component)
    {
        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO RecipeComponents (RecipeId, RawMaterialId, Percentage, LoadOrder, ToleranceMin, ToleranceMax)
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

    public async Task<bool> UpdateComponentAsync(int componentId, RecipeComponentDto component)
    {
        using var conn = CreateConnection();
        const string sql = @"
            UPDATE RecipeComponents 
            SET RawMaterialId = @RawMaterialId, Percentage = @Percentage, LoadOrder = @LoadOrder, 
                ToleranceMin = @ToleranceMin, ToleranceMax = @ToleranceMax
            WHERE Id = @Id";
        var rows = await conn.ExecuteAsync(sql, new
        {
            componentId,
            component.RawMaterialId,
            component.Percentage,
            component.LoadOrder,
            component.ToleranceMin,
            component.ToleranceMax
        });
        return rows > 0;
    }

    public async Task<bool> RemoveComponentAsync(int componentId)
    {
        using var conn = CreateConnection();
        const string sql = "DELETE FROM RecipeComponents WHERE Id = @Id";
        var rows = await conn.ExecuteAsync(sql, new { Id = componentId });
        return rows > 0;
    }

    public async Task<bool> UpdateStatusAsync(int id, string newStatus, int userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
            UPDATE Recipes 
            SET Status = @Status, UpdatedAt = GETDATE()
            WHERE Id = @Id AND Status != 'approved' AND Status != 'archived'";
        var rows = await conn.ExecuteAsync(sql, new { Status = newStatus, Id = id });
        return rows > 0;
    }

    public async Task<bool> ApproveAsync(int id, int userId)
    {
        // Проверка суммы 100%
        var total = await GetTotalPercentageAsync(id);
        if (Math.Abs(total - 100) > 0.01m)
            throw new InvalidOperationException($"Сумма долей компонентов должна быть 100%. Текущая: {total}%");

        // Проверка, что нет другой утверждённой рецептуры для того же продукта
        var recipe = await GetByIdAsync(id);
        if (recipe == null) return false;
        if (await IsAnyApprovedForProductAsync(recipe.ProductId, id))
            throw new InvalidOperationException("Для данного продукта уже существует утверждённая рецептура");

        using var conn = CreateConnection();
        const string sql = @"
            UPDATE Recipes 
            SET Status = 'approved', ApprovedAt = GETDATE(), ApprovedBy = @ApprovedBy, UpdatedAt = GETDATE()
            WHERE Id = @Id";
        var rows = await conn.ExecuteAsync(sql, new { Id = id, ApprovedBy = userId });
        return rows > 0;
    }

    public async Task<bool> ArchiveAsync(int id, int userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
            UPDATE Recipes 
            SET Status = 'archived', UpdatedAt = GETDATE()
            WHERE Id = @Id AND Status = 'approved'";
        var rows = await conn.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task<decimal> GetTotalPercentageAsync(int recipeId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT ISNULL(SUM(Percentage), 0) FROM RecipeComponents WHERE RecipeId = @RecipeId";
        return await conn.ExecuteScalarAsync<decimal>(sql, new { RecipeId = recipeId });
    }

    public async Task<bool> IsAnyApprovedForProductAsync(int productId, int excludeRecipeId = 0)
    {
        using var conn = CreateConnection();
        const string sql = @"
            SELECT COUNT(1) FROM Recipes 
            WHERE ProductId = @ProductId AND Status = 'approved' AND Id != @ExcludeId";
        var count = await conn.ExecuteScalarAsync<int>(sql, new { ProductId = productId, ExcludeId = excludeRecipeId });
        return count > 0;
    }
}