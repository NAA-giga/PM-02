using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using прогОпер.Models;
using прогОпер.Services;

namespace прогОпер.Repositories
{
    public class OperatorRepository : IOperatorRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public OperatorRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<ActiveBatchDto>> GetActiveBatchesAsync()
        {
            const string sql = @"
                SELECT 
                    pb.id,
                    pb.batch_number AS BatchNumber,
                    p.name AS ProductName,
                    e.line_number AS LineNumber,
                    pb.status,
                    bse.step_order AS CurrentStepOrder,
                    bse.step_name AS CurrentStepName,
                    bse.step_id AS CurrentStepId
                FROM production_batches pb
                JOIN products p ON pb.product_id = p.id
                LEFT JOIN tech_cards tc ON pb.tech_card_id = tc.id
                LEFT JOIN tech_steps ts ON ts.tech_card_id = tc.id AND ts.step_order = (
                    SELECT MIN(step_order) FROM tech_steps WHERE tech_card_id = tc.id
                )
                LEFT JOIN batch_step_execution bse ON bse.production_batch_id = pb.id 
                    AND bse.step_order = ts.step_order
                LEFT JOIN equipment e ON ts.equipment_id = e.id
                WHERE pb.status IN ('created', 'running')
                ORDER BY pb.start_time DESC";

            using var conn = _connectionFactory.CreateConnection();
            var batches = await conn.QueryAsync<ActiveBatchDto>(sql);
            return batches.AsList();
        }
        public async Task<BatchDetailsDto?> GetBatchDetailsAsync(int batchId)
        {
            const string sql = @"
        SELECT 
            pb.id, pb.batch_number, pb.tech_card_id, pb.status, pb.start_time
        FROM production_batches pb
        WHERE pb.id = @BatchId";

            using var conn = _connectionFactory.CreateConnection();
            var batch = await conn.QueryFirstOrDefaultAsync<BatchDetailsDto>(sql, new { BatchId = batchId });
            if (batch == null) return null;

            // Получаем все шаги (плановые + фактические)
            const string stepsSql = @"
        SELECT 
            ISNULL(bse.id, 0) AS Id,
            ts.id AS StepId,
            ts.step_order AS StepOrder,
            ts.step_name AS StepName,
            ts.step_type AS StepType,
            ts.equipment_id AS EquipmentId,
            e.name AS EquipmentName,
            ts.planned_temp_c AS PlannedTempC,
            ts.planned_pressure_bar AS PlannedPressureBar,
            ts.planned_duration_min AS PlannedDurationMin,
            ts.planned_speed_rpm AS PlannedSpeedRpm,
            ts.temp_tolerance_min AS TempToleranceMin,
            ts.temp_tolerance_max AS TempToleranceMax,
            ts.pressure_tolerance_min AS PressureToleranceMin,
            ts.pressure_tolerance_max AS PressureToleranceMax,
            ts.is_mandatory AS IsMandatory,
            ts.instruction AS Instruction,
            ISNULL(bse.status, 'pending') AS Status,
            bse.actual_temp_c AS ActualTempC,
            bse.actual_pressure_bar AS ActualPressureBar,
            bse.actual_duration_min AS ActualDurationMin,
            bse.actual_speed_rpm AS ActualSpeedRpm,
            ISNULL(bse.deviation_flag, 0) AS DeviationFlag,
            bse.deviation_description AS DeviationDescription,
            bse.operator_comment AS OperatorComment,
            bse.start_time AS StartTime,
            bse.end_time AS EndTime
        FROM tech_steps ts
        LEFT JOIN equipment e ON ts.equipment_id = e.id
        LEFT JOIN batch_step_execution bse ON bse.production_batch_id = @BatchId AND bse.step_id = ts.id
        WHERE ts.tech_card_id = @TechCardId
        ORDER BY ts.step_order";

            var steps = await conn.QueryAsync<StepExecutionDto>(stepsSql, new { BatchId = batchId, TechCardId = batch.TechCardId });
            batch.Steps = steps.AsList();
            return batch;
        }
    }
}
