using Dapper;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories;

public class ProductionBatchRepository : BaseRepository, IProductionBatchRepository
{
    public ProductionBatchRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<ProductionBatch>> GetActiveBatchesAsync()
    {
        using var conn = CreateConnection();
        const string sql = @"
            SELECT pb.*, p.name AS ProductName 
            FROM production_batches pb
            JOIN products p ON pb.product_id = p.id
            WHERE pb.status IN ('created', 'running')
            ORDER BY pb.start_time DESC";
        return await conn.QueryAsync<ProductionBatch>(sql);
    }

    public async Task<ProductionBatch?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM production_batches WHERE id = @Id";
        return await conn.QueryFirstOrDefaultAsync<ProductionBatch>(sql, new { Id = id });
    }

    public async Task<ProductionBatchResponseDto?> GetBatchWithStepsAsync(int id)
    {
        using var conn = CreateConnection();
        const string batchSql = @"
            SELECT pb.*, p.name AS ProductName, po.order_number AS OrderNumber
            FROM production_batches pb
            JOIN products p ON pb.product_id = p.id
            JOIN production_orders po ON pb.order_id = po.id
            WHERE pb.id = @Id";
        var batch = await conn.QueryFirstOrDefaultAsync<ProductionBatchResponseDto>(batchSql, new { Id = id });
        if (batch == null) return null;

        const string stepsSql = @"
            SELECT step_order, step_name, status, actual_temp_c, actual_pressure_bar, actual_duration_min, 
                   deviation_flag, deviation_description, operator_comment, start_time, end_time
            FROM batch_step_execution 
            WHERE production_batch_id = @Id 
            ORDER BY step_order";
        var steps = await conn.QueryAsync<BatchStepExecutionDto>(stepsSql, new { Id = id });
        batch.Steps = steps.ToList();
        return batch;
    }

    public async Task<int> StartBatchAsync(StartProductionBatchDto dto, int userId)
    {
        using var conn = CreateConnection();
        // Проверяем, что заказ существует и подтверждён
        var order = await conn.QueryFirstOrDefaultAsync<ProductionOrder>(
            "SELECT * FROM production_orders WHERE id = @OrderId AND status = 'confirmed'",
            new { dto.OrderId });
        if (order == null)
            throw new InvalidOperationException("Заказ не найден или не подтверждён");

        const string sql = @"
            INSERT INTO production_batches (
                batch_number, order_id, product_id, recipe_id, tech_card_id, 
                planned_quantity_kg, status, created_by, created_at, updated_at)
            VALUES (
                @BatchNumber, @OrderId, @ProductId, @RecipeId, @TechCardId, 
                @PlannedQuantityKg, 'created', @UserId, GETDATE(), GETDATE());
            SELECT CAST(SCOPE_IDENTITY() AS INT)";
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            dto.BatchNumber,
            dto.OrderId,
            order.ProductId,
            order.RecipeId,
            order.TechCardId,
            dto.PlannedQuantityKg,
            UserId = userId
        });
    }

    public async Task<bool> CompleteBatchAsync(int batchId, decimal actualQuantity, int userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
            UPDATE production_batches 
            SET status = 'completed', actual_quantity_kg = @ActualQuantity, end_time = GETDATE(), updated_at = GETDATE()
            WHERE id = @Id AND status = 'running'";
        var rows = await conn.ExecuteAsync(sql, new { Id = batchId, ActualQuantity = actualQuantity });
        return rows > 0;
    }

    public async Task<bool> UpdateBatchStatusAsync(int batchId, string status)
    {
        using var conn = CreateConnection();
        const string sql = "UPDATE production_batches SET status = @Status, updated_at = GETDATE() WHERE id = @Id";
        var rows = await conn.ExecuteAsync(sql, new { Id = batchId, Status = status });
        return rows > 0;
    }

    public async Task<bool> UpdateLabDecisionAsync(int batchId, string decision, string? reason, int userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
            UPDATE production_batches 
            SET lab_decision = @Decision, lab_decision_date = GETDATE(), lab_decision_by = @UserId, 
                lab_decision_reason = @Reason, updated_at = GETDATE(),
                status = CASE WHEN @Decision = 'approved' THEN 'completed' ELSE 'blocked' END
            WHERE id = @Id";
        var rows = await conn.ExecuteAsync(sql, new
        {
            Id = batchId,
            Decision = decision,
            Reason = reason,
            UserId = userId
        });
        return rows > 0;
    }
}