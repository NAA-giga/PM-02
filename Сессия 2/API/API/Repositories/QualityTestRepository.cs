using Dapper;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories;

public class QualityTestRepository : BaseRepository, IQualityTestRepository
{
    public QualityTestRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<QualityTest>> GetTestsByBatchIdAsync(int batchId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM quality_tests WHERE batch_id = @BatchId ORDER BY scheduled_date";
        return await conn.QueryAsync<QualityTest>(sql, new { BatchId = batchId });
    }

    public async Task<QualityTest?> GetTestWithResultsAsync(int testId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM quality_tests WHERE id = @TestId";
        return await conn.QueryFirstOrDefaultAsync<QualityTest>(sql, new { TestId = testId });
    }

    public async Task<int> CreateTestAsync(CreateQualityTestDto dto, int userId)
    {
        using var conn = CreateConnection();
        using var trans = conn.BeginTransaction();
        try
        {
            var testNumber = $"QT-{DateTime.Now:yyyyMMddHHmmss}";
            const string testSql = @"
                INSERT INTO quality_tests (
                    test_number, batch_id, test_type, status, scheduled_date, 
                    assigned_to, created_by, created_date)
                VALUES (
                    @TestNumber, @BatchId, @TestType, 'scheduled', @ScheduledDate, 
                    @AssignedTo, @UserId, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            var testId = await conn.ExecuteScalarAsync<int>(testSql, new
            {
                TestNumber = testNumber,
                dto.BatchId,
                dto.TestType,
                dto.ScheduledDate,
                dto.AssignedTo,
                UserId = userId
            }, trans);

            foreach (var param in dto.Parameters)
            {
                const string paramSql = @"
                    INSERT INTO quality_test_results (
                        test_id, parameter_name, standard_value_min, standard_value_max, 
                        standard_text, unit, is_critical)
                    VALUES (
                        @TestId, @ParameterName, @StandardValueMin, @StandardValueMax, 
                        @StandardText, @Unit, @IsCritical)";
                await conn.ExecuteAsync(paramSql, new
                {
                    TestId = testId,
                    param.ParameterName,
                    param.StandardValueMin,
                    param.StandardValueMax,
                    param.StandardText,
                    param.Unit,
                    param.IsCritical
                }, trans);
            }
            trans.Commit();
            return testId;
        }
        catch { trans.Rollback(); throw; }
    }

    public async Task<bool> EnterResultsAsync(EnterTestResultDto dto, int userId)
    {
        using var conn = CreateConnection();
        foreach (var res in dto.Results)
        {
            const string sql = @"
                UPDATE quality_test_results 
                SET measured_value = @MeasuredValue, analyst_comment = @Comment, measured_at = GETDATE(),
                    result = CASE 
                        WHEN standard_value_min IS NOT NULL AND standard_value_max IS NOT NULL 
                             AND @MeasuredValue BETWEEN standard_value_min AND standard_value_max THEN 'pass'
                        WHEN standard_value_min IS NULL AND standard_value_max IS NULL 
                             AND (standard_text IS NULL OR @MeasuredValue IS NOT NULL) THEN 'pass'
                        ELSE 'fail'
                    END
                WHERE id = @ResultId AND test_id = @TestId";
            await conn.ExecuteAsync(sql, new
            {
                res.MeasuredValue,
                Comment = res.AnalystComment,
                res.ResultId,
                dto.TestId
            });
        }

        // Проверяем, заполнены ли все параметры
        var allFilled = await conn.ExecuteScalarAsync<bool>(
            "SELECT COUNT(*) = 0 FROM quality_test_results WHERE test_id = @TestId AND measured_value IS NULL",
            new { dto.TestId });
        if (allFilled)
        {
            await conn.ExecuteAsync(
                "UPDATE quality_tests SET status = 'completed', completed_date = GETDATE() WHERE id = @TestId",
                new { dto.TestId });
        }
        return true;
    }

    public async Task<bool> CompleteTestAsync(int testId)
    {
        using var conn = CreateConnection();
        var rows = await conn.ExecuteAsync(
            "UPDATE quality_tests SET status = 'completed', completed_date = GETDATE() WHERE id = @Id",
            new { Id = testId });
        return rows > 0;
    }

    public async Task<bool> AreAllTestsCompletedAsync(int batchId)
    {
        using var conn = CreateConnection();
        var pending = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM quality_tests WHERE batch_id = @BatchId AND status != 'completed'",
            new { BatchId = batchId });
        return pending == 0;
    }
}