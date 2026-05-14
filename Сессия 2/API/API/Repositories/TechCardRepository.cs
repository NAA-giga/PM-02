using Dapper;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories;

public class TechCardRepository : BaseRepository, ITechCardRepository
{
    public TechCardRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<TechCard>> GetAllAsync(int? productId = null, string? status = null)
    {
        using var conn = CreateConnection();
        var sql = "SELECT * FROM tech_cards WHERE 1=1";
        if (productId.HasValue)
            sql += " AND product_id = @ProductId";
        if (!string.IsNullOrEmpty(status))
            sql += " AND status = @Status";
        sql += " ORDER BY created_at DESC";
        return await conn.QueryAsync<TechCard>(sql, new { ProductId = productId, Status = status });
    }

    public async Task<TechCard?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM tech_cards WHERE id = @Id";
        return await conn.QueryFirstOrDefaultAsync<TechCard>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(CreateTechCardDto dto, int userId)
    {
        using var conn = CreateConnection();
        using var trans = conn.BeginTransaction();

        try
        {
            const string cardSql = @"
         INSERT INTO tech_cards (product_id, version, name, description, status, created_by, created_at, updated_at)
         VALUES (@ProductId, @Version, @Name, @Description, 'draft', @UserId, GETDATE(), GETDATE());
         SELECT CAST(SCOPE_IDENTITY() AS INT)";
            var cardId = await conn.ExecuteScalarAsync<int>(cardSql, new
            {
                dto.ProductId,
                dto.Version,
                dto.Name,
                dto.Description,
                UserId = userId
            }, trans);

            if (dto.Steps != null && dto.Steps.Any())
            {
                const string stepSql = @"
             INSERT INTO tech_steps (tech_card_id, step_order, step_name, step_type, equipment_id,
                 planned_temp_c, planned_pressure_bar, planned_duration_min, planned_speed_rpm,
                 temp_tolerance_min, temp_tolerance_max, pressure_tolerance_min, pressure_tolerance_max,
                 is_mandatory, instruction, created_at)
             VALUES (@TechCardId, @StepOrder, @StepName, @StepType, @EquipmentId,
                 @PlannedTempC, @PlannedPressureBar, @PlannedDurationMin, @PlannedSpeedRpm,
                 @TempToleranceMin, @TempToleranceMax, @PressureToleranceMin, @PressureToleranceMax,
                 @IsMandatory, @Instruction, GETDATE())";

                foreach (var step in dto.Steps)
                {
                    await conn.ExecuteAsync(stepSql, new
                    {
                        TechCardId = cardId,
                        step.StepOrder,
                        step.StepName,
                        step.StepType,
                        step.EquipmentId,
                        step.PlannedTempC,
                        step.PlannedPressureBar,
                        step.PlannedDurationMin,
                        step.PlannedSpeedRpm,
                        step.TempToleranceMin,
                        step.TempToleranceMax,
                        step.PressureToleranceMin,
                        step.PressureToleranceMax,
                        step.IsMandatory,
                        step.Instruction
                    }, trans);
                }
            }

            trans.Commit();
            return cardId;
        }
        catch
        {
            trans.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, string newStatus, int userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
            UPDATE tech_cards 
            SET status = @Status, updated_at = GETDATE()
            WHERE id = @Id AND status NOT IN ('approved', 'replaced', 'archived')";
        var rows = await conn.ExecuteAsync(sql, new { Status = newStatus, Id = id });
        return rows > 0;
    }

    public async Task<bool> ApproveAsync(int id, int userId)
    {
        using var conn = CreateConnection();
        // Проверяем, что нет другой утверждённой карты для того же продукта
        const string checkSql = @"
            SELECT COUNT(1) FROM tech_cards 
            WHERE product_id = (SELECT product_id FROM tech_cards WHERE id = @Id)
            AND status = 'approved' AND id != @Id";
        var count = await conn.ExecuteScalarAsync<int>(checkSql, new { Id = id });
        if (count > 0)
            throw new InvalidOperationException("Для данного продукта уже есть утверждённая технологическая карта");

        const string sql = @"
            UPDATE tech_cards 
            SET status = 'approved', approved_at = GETDATE(), approved_by = @UserId, updated_at = GETDATE()
            WHERE id = @Id AND status = 'draft'";
        var rows = await conn.ExecuteAsync(sql, new { Id = id, UserId = userId });
        return rows > 0;
    }

    public async Task<bool> ArchiveAsync(int id, int userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
            UPDATE tech_cards 
            SET status = 'archived', updated_at = GETDATE()
            WHERE id = @Id AND status = 'approved'";
        var rows = await conn.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
    public async Task<bool> AddStepAsync(TechStep step)
    {
        using var conn = CreateConnection();
        const string sql = @"
        INSERT INTO tech_steps 
        (tech_card_id, step_order, step_name, step_type, equipment_id,
         planned_temp_c, planned_pressure_bar, planned_duration_min, planned_speed_rpm,
         temp_tolerance_min, temp_tolerance_max, pressure_tolerance_min, pressure_tolerance_max,
         is_mandatory, instruction, created_at)
        VALUES 
        (@TechCardId, @StepOrder, @StepName, @StepType, @EquipmentId,
         @PlannedTempC, @PlannedPressureBar, @PlannedDurationMin, @PlannedSpeedRpm,
         @TempToleranceMin, @TempToleranceMax, @PressureToleranceMin, @PressureToleranceMax,
         @IsMandatory, @Instruction, @CreatedAt);
        SELECT CAST(SCOPE_IDENTITY() AS INT)";
        step.CreatedAt = DateTime.UtcNow;
        var id = await conn.ExecuteScalarAsync<int>(sql, step);
        step.Id = id;
        return id > 0;
    }

    public async Task<bool> UpdateStepAsync(TechStep step)
    {
        using var conn = CreateConnection();
        const string sql = @"
        UPDATE tech_steps SET
            step_order = @StepOrder,
            step_name = @StepName,
            step_type = @StepType,
            equipment_id = @EquipmentId,
            planned_temp_c = @PlannedTempC,
            planned_pressure_bar = @PlannedPressureBar,
            planned_duration_min = @PlannedDurationMin,
            planned_speed_rpm = @PlannedSpeedRpm,
            temp_tolerance_min = @TempToleranceMin,
            temp_tolerance_max = @TempToleranceMax,
            pressure_tolerance_min = @PressureToleranceMin,
            pressure_tolerance_max = @PressureToleranceMax,
            is_mandatory = @IsMandatory,
            instruction = @Instruction
        WHERE id = @Id";
        var rows = await conn.ExecuteAsync(sql, step);
        return rows > 0;
    }

    public async Task<bool> DeleteStepAsync(int stepId)
    {
        using var conn = CreateConnection();
        const string sql = "DELETE FROM tech_steps WHERE id = @Id";
        var rows = await conn.ExecuteAsync(sql, new { Id = stepId });
        return rows > 0;
    }

    public async Task<TechStep?> GetStepByIdAsync(int stepId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM tech_steps WHERE id = @Id";
        return await conn.QueryFirstOrDefaultAsync<TechStep>(sql, new { Id = stepId });
    }
    public async Task<int> GetMaxVersionForProductAsync(int productId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT ISNULL(MAX(version), 0) FROM tech_cards WHERE product_id = @ProductId";
        return await conn.ExecuteScalarAsync<int>(sql, new { ProductId = productId });
    }

    public async Task<TechCardDto?> GetTechCardDetailsAsync(int id)
    {
        using var conn = CreateConnection();
        const string cardSql = @"
        SELECT 
            tc.id, tc.product_id AS ProductId, tc.version, tc.name, tc.description, tc.status,
            tc.approved_at AS ApprovedAt, tc.approved_by AS ApprovedBy, tc.created_by AS CreatedBy,
            tc.created_at AS CreatedAt, tc.updated_at AS UpdatedAt,
            p.name AS ProductName
        FROM tech_cards tc
        JOIN products p ON tc.product_id = p.id
        WHERE tc.id = @Id";
        var card = await conn.QueryFirstOrDefaultAsync<TechCardDto>(cardSql, new { Id = id });
        if (card == null) return null;

        const string stepsSql = @"
        SELECT 
            ts.id, ts.tech_card_id AS TechCardId, ts.step_order AS StepOrder, ts.step_name AS StepName,
            ts.step_type AS StepType, ts.equipment_id AS EquipmentId, e.name AS EquipmentName,
            ts.planned_temp_c AS PlannedTempC, ts.planned_pressure_bar AS PlannedPressureBar,
            ts.planned_duration_min AS PlannedDurationMin, ts.planned_speed_rpm AS PlannedSpeedRpm,
            ts.temp_tolerance_min AS TempToleranceMin, ts.temp_tolerance_max AS TempToleranceMax,
            ts.pressure_tolerance_min AS PressureToleranceMin, ts.pressure_tolerance_max AS PressureToleranceMax,
            ts.is_mandatory AS IsMandatory, ts.instruction AS Instruction
        FROM tech_steps ts
        LEFT JOIN equipment e ON ts.equipment_id = e.id
        WHERE ts.tech_card_id = @Id
        ORDER BY ts.step_order";
        var steps = await conn.QueryAsync<TechStepDto>(stepsSql, new { Id = id });
        card.Steps = steps.ToList();
        return card;
    }

    public async Task<bool> UpdateAsync(TechCard card)
    {
        using var conn = CreateConnection();
        const string sql = @"
        UPDATE tech_cards SET
            name = @Name,
            description = @Description,
            updated_at = @UpdatedAt
        WHERE id = @Id AND status = 'draft'";
        var rows = await conn.ExecuteAsync(sql, new { card.Id, card.Name, card.Description, card.UpdatedAt });
        return rows > 0;
    }
}
