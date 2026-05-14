using Dapper;
using API.Models.Entities;
using API.Models.DTOs;
using API.Repositories.Interfaces;

namespace API.Repositories
{
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

        public async Task<ProductionOrderDto?> GetOrderDetailsAsync(int id)
        {
            using var conn = CreateConnection();
            const string sql = @"
                SELECT 
                    o.id, o.order_number AS OrderNumber, o.product_id AS ProductId, o.recipe_id AS RecipeId,
                    o.tech_card_id AS TechCardId, o.planned_quantity_kg AS PlannedQuantityKg,
                    o.status, o.planned_start_date AS PlannedStartDate,
                    o.actual_start_date AS ActualStartDate, o.actual_end_date AS ActualEndDate,
                    o.created_at AS CreatedAt,
                    p.name AS ProductName,
                    r.name AS RecipeName,
                    tc.name AS TechCardName
                FROM production_orders o
                JOIN products p ON o.product_id = p.id
                JOIN recipes r ON o.recipe_id = r.id
                JOIN tech_cards tc ON o.tech_card_id = tc.id
                WHERE o.id = @Id";
            return await conn.QueryFirstOrDefaultAsync<ProductionOrderDto>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(ProductionOrder order)
        {
            using var conn = CreateConnection();
            const string sql = @"
                INSERT INTO production_orders (order_number, product_id, recipe_id, tech_card_id, planned_quantity_kg, status, planned_start_date, created_by, created_at)
                VALUES (@OrderNumber, @ProductId, @RecipeId, @TechCardId, @PlannedQuantityKg, @Status, @PlannedStartDate, @CreatedBy, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            return await conn.ExecuteScalarAsync<int>(sql, order);
        }

        public async Task<bool> UpdateAsync(ProductionOrder order)
        {
            using var conn = CreateConnection();
            const string sql = @"
                UPDATE production_orders 
                SET order_number = @OrderNumber, planned_quantity_kg = @PlannedQuantityKg,
                    planned_start_date = @PlannedStartDate
                WHERE id = @Id AND status = 'draft'";
            var rows = await conn.ExecuteAsync(sql, order);
            return rows > 0;
        }

        public async Task<bool> UpdateStatusAsync(int id, string newStatus, int userId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                UPDATE production_orders 
                SET status = @Status,
                    actual_start_date = CASE WHEN @Status = 'in_progress' AND actual_start_date IS NULL THEN GETDATE() ELSE actual_start_date END,
                    actual_end_date = CASE WHEN @Status = 'completed' THEN GETDATE() ELSE actual_end_date END
                WHERE id = @Id";
            var rows = await conn.ExecuteAsync(sql, new { Id = id, Status = newStatus });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            const string sql = "DELETE FROM production_orders WHERE id = @Id AND status = 'draft'";
            var rows = await conn.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }

        public async Task<bool> IsOrderNumberUniqueAsync(string orderNumber, int excludeId = 0)
        {
            using var conn = CreateConnection();
            const string sql = "SELECT COUNT(1) FROM production_orders WHERE order_number = @OrderNumber AND id != @ExcludeId";
            var count = await conn.ExecuteScalarAsync<int>(sql, new { OrderNumber = orderNumber, ExcludeId = excludeId });
            return count == 0;
        }

        public async Task<IEnumerable<ProductionOrder>> GetOrdersByProductAsync(int productId)
        {
            using var conn = CreateConnection();
            const string sql = "SELECT * FROM production_orders WHERE product_id = @ProductId ORDER BY planned_start_date DESC";
            return await conn.QueryAsync<ProductionOrder>(sql, new { ProductId = productId });
        }
    }
}