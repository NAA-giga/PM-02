using Dapper;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories
{
    public class DeviationRepository : BaseRepository, IDeviationRepository
    {
        public DeviationRepository(IConfiguration configuration) : base(configuration) { }

        /// <summary>
        /// Получить все отклонения с фильтрацией по дате
        /// </summary>
        public async Task<IEnumerable<DeviationDto>> GetAllAsync(DateTime? from = null, DateTime? to = null)
        {
            using var conn = CreateConnection();
            var sql = @"
                SELECT 
                    d.id,
                    d.deviation_type AS DeviationType,
                    d.severity,
                    d.description,
                    d.planned_value AS PlannedValue,
                    d.actual_value AS ActualValue,
                    d.parameter_name AS ParameterName,
                    d.created_at AS CreatedAt,
                    pb.batch_number AS BatchNumber,
                    bse.step_order AS StepOrder,
                    bse.step_name AS StepName
                FROM deviations d
                JOIN production_batches pb ON d.production_batch_id = pb.id
                LEFT JOIN batch_step_execution bse ON d.step_execution_id = bse.id
                WHERE 1=1";
            if (from.HasValue)
                sql += " AND d.created_at >= @From";
            if (to.HasValue)
                sql += " AND d.created_at <= @To";
            sql += " ORDER BY d.created_at DESC";
            return await conn.QueryAsync<DeviationDto>(sql, new { From = from, To = to });
        }

        /// <summary>
        /// Получить отклонения по ID производственной партии
        /// </summary>
        public async Task<IEnumerable<DeviationDto>> GetByBatchIdAsync(int batchId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                SELECT 
                    d.id,
                    d.deviation_type AS DeviationType,
                    d.severity,
                    d.description,
                    d.planned_value AS PlannedValue,
                    d.actual_value AS ActualValue,
                    d.parameter_name AS ParameterName,
                    d.created_at AS CreatedAt,
                    pb.batch_number AS BatchNumber,
                    bse.step_order AS StepOrder,
                    bse.step_name AS StepName
                FROM deviations d
                JOIN production_batches pb ON d.production_batch_id = pb.id
                LEFT JOIN batch_step_execution bse ON d.step_execution_id = bse.id
                WHERE d.production_batch_id = @BatchId
                ORDER BY d.created_at DESC";
            return await conn.QueryAsync<DeviationDto>(sql, new { BatchId = batchId });
        }

        /// <summary>
        /// Создать новое отклонение
        /// </summary>
        public async Task<int> CreateAsync(ReportDeviationDto dto, int userId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                INSERT INTO deviations 
                (production_batch_id, step_execution_id, deviation_type, severity, 
                 description, planned_value, actual_value, parameter_name, 
                 created_by, created_at)
                VALUES 
                (@ProductionBatchId, @StepExecutionId, @DeviationType, @Severity,
                 @Description, @PlannedValue, @ActualValue, @ParameterName,
                 @UserId, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                dto.ProductionBatchId,
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
    }
}