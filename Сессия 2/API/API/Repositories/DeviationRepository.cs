using Dapper;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories;

public class DeviationRepository : BaseRepository, IDeviationRepository
{
    public DeviationRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<int> CreateAsync(ReportDeviationDto dto, int userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO deviations (
                production_batch_id, step_execution_id, deviation_type, severity, 
                description, planned_value, actual_value, parameter_name, 
                created_by, created_at)
            VALUES (
                @BatchId, @StepExecutionId, @DeviationType, @Severity, 
                @Description, @PlannedValue, @ActualValue, @ParameterName, 
                @UserId, GETDATE());
            SELECT CAST(SCOPE_IDENTITY() AS INT)";
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            BatchId = dto.ProductionBatchId,
            dto.StepExecutionId,
            dto.DeviationType,
            dto.Severity,
            dto.Description,
            dto.PlannedValue,
            dto.ActualValue,
            dto.ParameterName,
            UserId = userId
        });
    }

    public async Task<IEnumerable<Deviation>> GetByBatchIdAsync(int batchId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM deviations WHERE production_batch_id = @BatchId ORDER BY created_at DESC";
        return await conn.QueryAsync<Deviation>(sql, new { BatchId = batchId });
    }
}