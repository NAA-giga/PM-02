using Dapper;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories;

public class ProductionOrderRepository : BaseRepository, IProductionOrderRepository
{
    public ProductionOrderRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<ProductionOrder>> GetAllAsync()
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM production_orders ORDER BY planned_start_date DESC";
        return await conn.QueryAsync<ProductionOrder>(sql);
    }

    public async Task<ProductionOrder?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM production_orders WHERE id = @Id";
        return await conn.QueryFirstOrDefaultAsync<ProductionOrder>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(CreateProductionOrderDto dto, int userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO production_orders (
                order_number, product_id, recipe_id, tech_card_id, 
                planned_quantity_kg, status, planned_start_date, created_by, created_at)
            VALUES (
                @OrderNumber, @ProductId, @RecipeId, @TechCardId, 
                @PlannedQuantityKg, 'draft', @PlannedStartDate, @UserId, GETDATE());
            SELECT CAST(SCOPE_IDENTITY() AS INT)";
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            dto.OrderNumber,
            dto.ProductId,
            dto.RecipeId,
            dto.TechCardId,
            dto.PlannedQuantityKg,
            dto.PlannedStartDate,
            UserId = userId
        });
    }

    public async Task<bool> UpdateStatusAsync(int id, string status, int userId)
    {
        using var conn = CreateConnection();
        const string sql = "UPDATE production_orders SET status = @Status WHERE id = @Id";
        var rows = await conn.ExecuteAsync(sql, new { Status = status, Id = id });
        return rows > 0;
    }
}