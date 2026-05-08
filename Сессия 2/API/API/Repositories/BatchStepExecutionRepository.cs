using Dapper;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories;

public class BatchStepExecutionRepository : BaseRepository, IBatchStepExecutionRepository
{
    public BatchStepExecutionRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<BatchStepExecution>> GetStepsByBatchIdAsync(int batchId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM batch_step_execution WHERE production_batch_id = @BatchId ORDER BY step_order";
        return await conn.QueryAsync<BatchStepExecution>(sql, new { BatchId = batchId });
    }

    public async Task<bool> StartStepAsync(int batchId, int stepOrder, int userId)
    {
        using var conn = CreateConnection();
        // Проверяем, не начат ли уже шаг
        var existing = await conn.QueryFirstOrDefaultAsync<BatchStepExecution>(
            "SELECT * FROM batch_step_execution WHERE production_batch_id = @BatchId AND step_order = @StepOrder",
            new { BatchId = batchId, StepOrder = stepOrder });
        if (existing != null && existing.StartTime != null)
            return false; // уже начат

        const string sql = @"
            IF EXISTS (SELECT 1 FROM batch_step_execution WHERE production_batch_id = @BatchId AND step_order = @StepOrder)
                UPDATE batch_step_execution 
                SET start_time = GETDATE(), started_by = @UserId, status = 'running'
                WHERE production_batch_id = @BatchId AND step_order = @StepOrder
            ELSE
                INSERT INTO batch_step_execution (
                    production_batch_id, step_id, step_order, step_name, status, start_time, started_by)
                SELECT @BatchId, ts.id, ts.step_order, ts.step_name, 'running', GETDATE(), @UserId
                FROM tech_steps ts
                WHERE ts.tech_card_id = (SELECT tech_card_id FROM production_batches WHERE id = @BatchId) 
                  AND ts.step_order = @StepOrder";
        var rows = await conn.ExecuteAsync(sql, new { BatchId = batchId, StepOrder = stepOrder, UserId = userId });
        return rows > 0;
    }

    public async Task<bool> CompleteStepAsync(int batchId, int stepOrder, PerformStepDto data, int userId)
    {
        using var conn = CreateConnection();
        // Получаем плановые допуски из tech_steps для определения отклонения
        var step = await conn.QueryFirstOrDefaultAsync<TechStep>(@"
            SELECT ts.* FROM tech_steps ts
            JOIN production_batches pb ON pb.tech_card_id = ts.tech_card_id
            WHERE pb.id = @BatchId AND ts.step_order = @StepOrder",
            new { BatchId = batchId, StepOrder = stepOrder });

        bool deviationFlag = false;
        string? deviationDesc = null;

        if (step != null)
        {
            if (data.ActualTempC.HasValue && step.PlannedTempC.HasValue)
            {
                var min = step.PlannedTempC - (step.TempToleranceMin ?? 0);
                var max = step.PlannedTempC + (step.TempToleranceMax ?? 0);
                if (data.ActualTempC < min || data.ActualTempC > max)
                {
                    deviationFlag = true;
                    deviationDesc = $"Температура {data.ActualTempC}°C выходит за пределы [{min}..{max}]";
                }
            }
            if (data.ActualPressureBar.HasValue && step.PlannedPressureBar.HasValue)
            {
                var min = step.PlannedPressureBar - (step.PressureToleranceMin ?? 0);
                var max = step.PlannedPressureBar + (step.PressureToleranceMax ?? 0);
                if (data.ActualPressureBar < min || data.ActualPressureBar > max)
                {
                    deviationFlag = true;
                    deviationDesc = $"Давление {data.ActualPressureBar} бар выходит за пределы [{min}..{max}]";
                }
            }
        }

        const string sql = @"
            UPDATE batch_step_execution 
            SET actual_temp_c = @ActualTempC, actual_pressure_bar = @ActualPressureBar, 
                actual_duration_min = @ActualDurationMin,
                deviation_flag = @DeviationFlag, deviation_description = @DeviationDesc, 
                operator_comment = @OperatorComment, end_time = GETDATE(), 
                completed_by = @UserId, status = 'completed'
            WHERE production_batch_id = @BatchId AND step_order = @StepOrder 
              AND start_time IS NOT NULL";
        var rows = await conn.ExecuteAsync(sql, new
        {
            data.ActualTempC,
            data.ActualPressureBar,
            data.ActualDurationMin,
            DeviationFlag = deviationFlag,
            DeviationDesc = deviationDesc,
            data.OperatorComment,
            BatchId = batchId,
            StepOrder = stepOrder,
            UserId = userId
        });

        // Если шаг успешно завершён, проверяем, не все ли шаги выполнены
        if (rows > 0)
        {
            var totalSteps = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM tech_steps WHERE tech_card_id = (SELECT tech_card_id FROM production_batches WHERE id = @BatchId)",
                new { BatchId = batchId });
            var completedSteps = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM batch_step_execution WHERE production_batch_id = @BatchId AND status = 'completed'",
                new { BatchId = batchId });
            if (totalSteps == completedSteps)
            {
                await conn.ExecuteAsync(
                    "UPDATE production_batches SET status = 'quality_control' WHERE id = @BatchId",
                    new { BatchId = batchId });
            }
        }
        return rows > 0;
    }
}