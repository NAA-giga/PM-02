using Dapper;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories
{
    public class BatchStepExecutionRepository : BaseRepository, IBatchStepExecutionRepository
    {
        public BatchStepExecutionRepository(IConfiguration config) : base(config) { }

        public async Task<bool> StartStepAsync(int batchId, int stepOrder, int userId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                INSERT INTO batch_step_execution 
                (production_batch_id, step_id, step_order, step_name, status, start_time, started_by)
                SELECT @BatchId, ts.id, ts.step_order, ts.step_name, 'running', GETDATE(), @UserId
                FROM tech_steps ts
                JOIN production_batches pb ON pb.tech_card_id = ts.tech_card_id
                WHERE pb.id = @BatchId AND ts.step_order = @StepOrder";
            var rows = await conn.ExecuteAsync(sql, new { BatchId = batchId, StepOrder = stepOrder, UserId = userId });
            return rows > 0;
        }

        public async Task<bool> CompleteStepAsync(int batchId, int stepOrder, PerformStepDto data, int userId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                UPDATE batch_step_execution 
                SET actual_temp_c = @ActualTempC,
                    actual_pressure_bar = @ActualPressureBar,
                    actual_duration_min = @ActualDurationMin,
                    operator_comment = @OperatorComment,
                    end_time = GETDATE(),
                    completed_by = @UserId,
                    status = 'completed'
                WHERE production_batch_id = @BatchId AND step_order = @StepOrder AND start_time IS NOT NULL";
            var rows = await conn.ExecuteAsync(sql, new
            {
                data.ActualTempC,
                data.ActualPressureBar,
                data.ActualDurationMin,
                data.OperatorComment,
                BatchId = batchId,
                StepOrder = stepOrder,
                UserId = userId
            });
            return rows > 0;
        }
    }
}