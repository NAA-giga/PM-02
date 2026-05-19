using Dapper;
using ПрогЛабор.Models;
using ПрогЛабор.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace LaboratoryApp.Repositories
{
    public class LabRepository : ILabRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IEventLogger _eventLogger;

        public LabRepository(IDbConnectionFactory connectionFactory, IEventLogger eventLogger)
        {
            _connectionFactory = connectionFactory;
            _eventLogger = eventLogger;
        }

        private IDbConnection CreateConnection() => _connectionFactory.CreateConnection();

        // ============================================================
        // 1. Сырьё
        // ============================================================

        public async Task<List<RawMaterialBatchDto>> GetRawMaterialBatchesAsync(string? status = null)
        {
            using var conn = CreateConnection();
            var sql = @"
                SELECT 
                    rmb.id,
                    rmb.batch_number AS BatchNumber,
                    rm.name AS RawMaterialName,
                    rmb.supplier_batch_number AS SupplierBatchNumber,
                    rmb.supplier_name AS SupplierName,
                    rmb.quantity,
                    rmb.unit,
                    rmb.receipt_date AS ReceiptDate,
                    rmb.expiration_date AS ExpirationDate,
                    rmb.lab_status AS LabStatus,
                    rmb.storage_location AS StorageLocation,
                    rmb.created_at AS CreatedAt
                FROM raw_material_batches rmb
                JOIN raw_materials rm ON rmb.raw_material_id = rm.id
                WHERE 1=1";
            if (!string.IsNullOrEmpty(status))
                sql += " AND rmb.lab_status = @Status";
            else
                sql += " AND rmb.lab_status IN ('pending', 'in_progress')";
            sql += " ORDER BY rmb.receipt_date DESC";
            return (await conn.QueryAsync<RawMaterialBatchDto>(sql, new { Status = status })).ToList();
        }

        public async Task<RawMaterialBatchDto?> GetRawMaterialBatchByIdAsync(int id)
        {
            using var conn = CreateConnection();
            const string sql = @"
                SELECT 
                    rmb.id,
                    rmb.batch_number AS BatchNumber,
                    rm.name AS RawMaterialName,
                    rmb.supplier_batch_number AS SupplierBatchNumber,
                    rmb.supplier_name AS SupplierName,
                    rmb.quantity,
                    rmb.unit,
                    rmb.receipt_date AS ReceiptDate,
                    rmb.expiration_date AS ExpirationDate,
                    rmb.lab_status AS LabStatus,
                    rmb.storage_location AS StorageLocation
                FROM raw_material_batches rmb
                JOIN raw_materials rm ON rmb.raw_material_id = rm.id
                WHERE rmb.id = @Id";
            return await conn.QueryFirstOrDefaultAsync<RawMaterialBatchDto>(sql, new { Id = id });
        }

        public async Task<int> CreateRawMaterialTestAsync(int batchId, string testType, int userId)
        {
            using var conn = CreateConnection();
            // Проверяем, есть ли незавершённое испытание для этой партии
            var existing = await conn.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(1) FROM raw_material_tests WHERE raw_material_batch_id = @BatchId AND status IN ('scheduled', 'in_progress')",
                new { BatchId = batchId });
            if (existing > 0)
                throw new InvalidOperationException("Для этой партии уже есть незавершённое испытание");

            var testNumber = $"RMT-{DateTime.Now:yyyyMMddHHmmss}";
            const string sql = @"
                INSERT INTO raw_material_tests 
                (test_number, raw_material_batch_id, test_type, status, created_date, created_by)
                VALUES 
                (@TestNumber, @BatchId, @TestType, 'in_progress', GETDATE(), @UserId);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            var testId = await conn.ExecuteScalarAsync<int>(sql, new
            {
                TestNumber = testNumber,
                BatchId = batchId,
                TestType = testType,
                UserId = userId
            });

            // Обновляем статус партии на "in_progress"
            await conn.ExecuteAsync("UPDATE raw_material_batches SET lab_status = 'in_progress' WHERE id = @Id", new { Id = batchId });

            // Логируем событие
            await _eventLogger.LogAsync("test_created", "raw_material_test", testId, $"Создано испытание {testNumber} для партии сырья {batchId}", userId);
            return testId;
        }

        public async Task<RawMaterialTestDto?> GetRawMaterialTestByIdAsync(int testId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                SELECT 
                    t.id,
                    t.raw_material_batch_id AS RawMaterialBatchId,
                    t.test_number AS TestNumber,
                    t.test_type AS TestType,
                    t.status,
                    t.created_date AS CreatedDate,
                    t.completed_date AS CompletedDate,
                    t.assigned_to AS AssignedTo,
                    u.full_name AS AssignedToName,
                    t.decision,
                    t.decision_reason AS DecisionReason
                FROM raw_material_tests t
                LEFT JOIN users u ON t.assigned_to = u.id
                WHERE t.id = @TestId";
            return await conn.QueryFirstOrDefaultAsync<RawMaterialTestDto>(sql, new { TestId = testId });
        }

        public async Task<List<RawMaterialTestResultDto>> GetRawMaterialTestResultsAsync(int testId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                SELECT 
                    id,
                    test_id AS TestId,
                    parameter_name AS ParameterName,
                    measured_value AS MeasuredValue,
                    standard_value_min AS StandardValueMin,
                    standard_value_max AS StandardValueMax,
                    standard_text AS StandardText,
                    unit,
                    result,
                    is_critical AS IsCritical,
                    analyst_comment AS AnalystComment,
                    measured_at AS MeasuredAt
                FROM raw_material_test_results
                WHERE test_id = @TestId
                ORDER BY id";
            return (await conn.QueryAsync<RawMaterialTestResultDto>(sql, new { TestId = testId })).ToList();
        }

        public async Task<bool> SaveRawMaterialTestResultAsync(RawMaterialTestResultDto result)
        {
            using var conn = CreateConnection();
            // Вычисляем Result на основе MeasuredValue и стандартов
            result.Result = ComputeResult(result);
            result.MeasuredAt = DateTime.Now;

            const string sql = @"
                IF EXISTS (SELECT 1 FROM raw_material_test_results WHERE id = @Id)
                    UPDATE raw_material_test_results SET
                        measured_value = @MeasuredValue,
                        analyst_comment = @AnalystComment,
                        result = @Result,
                        measured_at = @MeasuredAt
                    WHERE id = @Id
                ELSE
                    INSERT INTO raw_material_test_results 
                    (test_id, parameter_name, measured_value, standard_value_min, standard_value_max, standard_text, unit, result, is_critical, analyst_comment, measured_at)
                    VALUES 
                    (@TestId, @ParameterName, @MeasuredValue, @StandardValueMin, @StandardValueMax, @StandardText, @Unit, @Result, @IsCritical, @AnalystComment, @MeasuredAt)";
            var rows = await conn.ExecuteAsync(sql, result);
            if (rows > 0)
            {
                await _eventLogger.LogAsync("result_saved", "raw_material_test", result.TestId, $"Сохранён параметр {result.ParameterName} = {result.MeasuredValue}", null);
            }
            return rows > 0;
        }

        public async Task<bool> CompleteRawMaterialTestAsync(int testId, int userId)
        {
            using var conn = CreateConnection();
            // Проверяем, что все параметры заполнены (measured_value IS NOT NULL)
            var incompleteCount = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1) FROM raw_material_test_results 
                  WHERE test_id = @TestId AND measured_value IS NULL",
                new { TestId = testId });
            if (incompleteCount > 0)
                throw new InvalidOperationException("Не все параметры заполнены. Завершить испытание невозможно.");

            const string sql = @"
                UPDATE raw_material_tests 
                SET status = 'completed', completed_date = GETDATE(), assigned_to = @UserId
                WHERE id = @TestId AND status != 'completed'";
            var rows = await conn.ExecuteAsync(sql, new { TestId = testId, UserId = userId });
            if (rows > 0)
            {
                await _eventLogger.LogAsync("test_completed", "raw_material_test", testId, $"Испытание {testId} завершено", userId);
            }
            return rows > 0;
        }

        public async Task<bool> SetRawMaterialBatchDecisionAsync(int batchId, string decision, string? reason, int userId)
        {
            using var conn = CreateConnection();
            // Проверяем, что есть хотя бы одно завершённое испытание
            var completedCount = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1) FROM raw_material_tests 
                  WHERE raw_material_batch_id = @BatchId AND status = 'completed'",
                new { BatchId = batchId });
            if (completedCount == 0)
                throw new InvalidOperationException("Нет завершённых испытаний для этой партии. Невозможно принять решение.");

            const string sql = @"
                UPDATE raw_material_batches 
                SET lab_status = @Decision, 
                    lab_decision_date = GETDATE(), 
                    lab_decision_by = @UserId, 
                    lab_decision_reason = @Reason
                WHERE id = @BatchId";
            var rows = await conn.ExecuteAsync(sql, new { BatchId = batchId, Decision = decision, Reason = reason, UserId = userId });
            if (rows > 0)
            {
                await _eventLogger.LogAsync("batch_decision", "raw_material_batch", batchId, $"Партия сырья {batchId} {decision} (причина: {reason ?? "нет"})", userId);
            }
            return rows > 0;
        }

        public async Task<bool> IsRawMaterialTestCompleted(int batchId)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1) FROM raw_material_tests 
                  WHERE raw_material_batch_id = @BatchId AND status = 'completed'",
                new { BatchId = batchId });
            return count > 0;
        }

        // ============================================================
        // 2. Готовая продукция
        // ============================================================

        public async Task<List<ProductBatchForLabDto>> GetProductBatchesForLabAsync()
        {
            using var conn = CreateConnection();
            const string sql = @"
                SELECT 
                    pb.id,
                    pb.batch_number AS BatchNumber,
                    p.name AS ProductName,
                    pb.planned_quantity_kg AS PlannedQuantityKg,
                    pb.actual_quantity_kg AS ActualQuantityKg,
                    pb.start_time AS StartTime,
                    pb.end_time AS EndTime,
                    pb.status,
                    pb.lab_decision AS LabDecision,
                    pb.lab_decision_date AS LabDecisionDate,
                    pb.lab_decision_reason AS LabDecisionReason
                FROM production_batches pb
                JOIN products p ON pb.product_id = p.id
                WHERE pb.status = 'quality_control'
                   OR (pb.status = 'completed' AND pb.lab_decision IS NULL)
                ORDER BY pb.start_time DESC";
            return (await conn.QueryAsync<ProductBatchForLabDto>(sql)).ToList();
        }

        public async Task<int> CreateQualityTestAsync(int batchId, string testType, int userId)
        {
            using var conn = CreateConnection();
            // Проверяем незавершённое испытание
            var existing = await conn.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(1) FROM quality_tests WHERE batch_id = @BatchId AND status IN ('scheduled', 'in_progress')",
                new { BatchId = batchId });
            if (existing > 0)
                throw new InvalidOperationException("Для этой партии уже есть незавершённое испытание");

            var testNumber = $"QT-{DateTime.Now:yyyyMMddHHmmss}";
            const string sql = @"
                INSERT INTO quality_tests 
                (test_number, batch_id, test_type, status, created_date, scheduled_date, created_by)
                VALUES 
                (@TestNumber, @BatchId, @TestType, 'in_progress', GETDATE(), GETDATE(), @UserId);
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            var testId = await conn.ExecuteScalarAsync<int>(sql, new
            {
                TestNumber = testNumber,
                BatchId = batchId,
                TestType = testType,
                UserId = userId
            });
            await _eventLogger.LogAsync("test_created", "quality_test", testId, $"Создано испытание {testNumber} для партии продукции {batchId}", userId);
            return testId;
        }

        public async Task<QualityTestDto?> GetQualityTestByIdAsync(int testId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                SELECT 
                    t.id,
                    t.batch_id AS BatchId,
                    t.test_number AS TestNumber,
                    t.test_type AS TestType,
                    t.status,
                    t.priority,
                    t.created_date AS CreatedDate,
                    t.scheduled_date AS ScheduledDate,
                    t.completed_date AS CompletedDate,
                    t.assigned_to AS AssignedTo,
                    u.full_name AS AssignedToName
                FROM quality_tests t
                LEFT JOIN users u ON t.assigned_to = u.id
                WHERE t.id = @TestId";
            return await conn.QueryFirstOrDefaultAsync<QualityTestDto>(sql, new { TestId = testId });
        }

        public async Task<List<QualityTestResultDto>> GetQualityTestResultsAsync(int testId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                SELECT 
                    id,
                    test_id AS TestId,
                    parameter_name AS ParameterName,
                    measured_value AS MeasuredValue,
                    standard_value_min AS StandardValueMin,
                    standard_value_max AS StandardValueMax,
                    standard_text AS StandardText,
                    unit,
                    result,
                    is_critical AS IsCritical,
                    analyst_comment AS AnalystComment,
                    measured_at AS MeasuredAt
                FROM quality_test_results
                WHERE test_id = @TestId
                ORDER BY id";
            return (await conn.QueryAsync<QualityTestResultDto>(sql, new { TestId = testId })).ToList();
        }

        public async Task<bool> SaveQualityTestResultAsync(QualityTestResultDto result)
        {
            using var conn = CreateConnection();
            result.Result = ComputeQualityResult(result);
            result.MeasuredAt = DateTime.Now;

            const string sql = @"
                IF EXISTS (SELECT 1 FROM quality_test_results WHERE id = @Id)
                    UPDATE quality_test_results SET
                        measured_value = @MeasuredValue,
                        analyst_comment = @AnalystComment,
                        result = @Result,
                        measured_at = @MeasuredAt
                    WHERE id = @Id
                ELSE
                    INSERT INTO quality_test_results 
                    (test_id, parameter_name, measured_value, standard_value_min, standard_value_max, standard_text, unit, result, is_critical, analyst_comment, measured_at)
                    VALUES 
                    (@TestId, @ParameterName, @MeasuredValue, @StandardValueMin, @StandardValueMax, @StandardText, @Unit, @Result, @IsCritical, @AnalystComment, @MeasuredAt)";
            var rows = await conn.ExecuteAsync(sql, result);
            if (rows > 0)
            {
                await _eventLogger.LogAsync("result_saved", "quality_test", result.TestId, $"Сохранён параметр {result.ParameterName} = {result.MeasuredValue}", null);
            }
            return rows > 0;
        }

        public async Task<bool> CompleteQualityTestAsync(int testId, int userId)
        {
            using var conn = CreateConnection();
            var incompleteCount = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1) FROM quality_test_results 
                  WHERE test_id = @TestId AND measured_value IS NULL",
                new { TestId = testId });
            if (incompleteCount > 0)
                throw new InvalidOperationException("Не все параметры заполнены. Завершить испытание невозможно.");

            const string sql = @"
                UPDATE quality_tests 
                SET status = 'completed', completed_date = GETDATE(), assigned_to = @UserId
                WHERE id = @TestId";
            var rows = await conn.ExecuteAsync(sql, new { TestId = testId, UserId = userId });
            if (rows > 0)
            {
                await _eventLogger.LogAsync("test_completed", "quality_test", testId, $"Испытание {testId} завершено", userId);
            }
            return rows > 0;
        }

        public async Task<bool> SetProductBatchDecisionAsync(int batchId, string decision, string? reason, int userId)
        {
            using var conn = CreateConnection();
            // Проверяем, что есть завершённое испытание
            var completedCount = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1) FROM quality_tests 
                  WHERE batch_id = @BatchId AND status = 'completed'",
                new { BatchId = batchId });
            if (completedCount == 0)
                throw new InvalidOperationException("Нет завершённых испытаний для этой партии. Невозможно принять решение.");

            const string sql = @"
                UPDATE production_batches 
                SET lab_decision = @Decision, 
                    lab_decision_date = GETDATE(), 
                    lab_decision_by = @UserId, 
                    lab_decision_reason = @Reason,
                    status = CASE WHEN @Decision = 'approved' THEN 'completed' ELSE 'blocked' END
                WHERE id = @BatchId";
            var rows = await conn.ExecuteAsync(sql, new { BatchId = batchId, Decision = decision, Reason = reason, UserId = userId });
            if (rows > 0)
            {
                await _eventLogger.LogAsync("batch_decision", "product_batch", batchId, $"Партия продукции {batchId} {decision} (причина: {reason ?? "нет"})", userId);
            }
            return rows > 0;
        }

        public async Task<bool> IsProductTestCompleted(int batchId)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1) FROM quality_tests 
                  WHERE batch_id = @BatchId AND status = 'completed'",
                new { BatchId = batchId });
            return count > 0;
        }

        // ============================================================
        // Вспомогательные методы
        // ============================================================

        private string ComputeResult(RawMaterialTestResultDto result)
        {
            if (!result.MeasuredValue.HasValue)
                return "not_tested";
            // Если заданы числовые диапазоны
            if (result.StandardValueMin.HasValue && result.StandardValueMax.HasValue)
            {
                if (result.MeasuredValue.Value >= result.StandardValueMin.Value && result.MeasuredValue.Value <= result.StandardValueMax.Value)
                    return "pass";
                else
                    return "fail";
            }
            // Если задан текстовый стандарт (например, "отсутствие примесей") – считаем pass, если есть комментарий или просто проверяем наличие
            if (!string.IsNullOrEmpty(result.StandardText))
                return "pass"; // упрощённо: по умолчанию pass, если нет числового диапазона
            return "pass";
        }

        private string ComputeQualityResult(QualityTestResultDto result)
        {
            if (!result.MeasuredValue.HasValue)
                return "not_tested";
            if (result.StandardValueMin.HasValue && result.StandardValueMax.HasValue)
            {
                if (result.MeasuredValue.Value >= result.StandardValueMin.Value && result.MeasuredValue.Value <= result.StandardValueMax.Value)
                    return "pass";
                else
                    return "fail";
            }
            if (!string.IsNullOrEmpty(result.StandardText))
                return "pass";
            return "pass";
        }
    }
}