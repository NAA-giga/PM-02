using Dapper;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories;

public class TechCardRepository : BaseRepository, ITechCardRepository
{
    public TechCardRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<TechCard>> GetAllAsync()
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM tech_cards ORDER BY created_at DESC";
        return await conn.QueryAsync<TechCard>(sql);
    }

    public async Task<TechCard?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM tech_cards WHERE id = @Id";
        return await conn.QueryFirstOrDefaultAsync<TechCard>(sql, new { Id = id });
    }

    public async Task<TechCardResponseDto?> GetDetailsAsync(int id)
    {
        using var conn = CreateConnection();
        const string sql = @"
            SELECT tc.*, p.name AS ProductName 
            FROM tech_cards tc
            JOIN products p ON tc.product_id = p.id
            WHERE tc.id = @Id";
        var card = await conn.QueryFirstOrDefaultAsync<TechCardResponseDto>(sql, new { Id = id });
        if (card == null) return null;

        const string stepsSql = @"
            SELECT ts.*, e.name AS EquipmentName 
            FROM tech_steps ts
            LEFT JOIN equipment e ON ts.equipment_id = e.id
            WHERE ts.tech_card_id = @Id
            ORDER BY ts.step_order";
        var steps = await conn.QueryAsync<TechStepResponseDto>(stepsSql, new { Id = id });
        card.Steps = steps.ToList();
        return card;
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

            foreach (var step in dto.Steps)
            {
                const string stepSql = @"
                    INSERT INTO tech_steps (
                        tech_card_id, step_order, step_name, step_type, equipment_id,
                        planned_temp_c, planned_pressure_bar, planned_duration_min, planned_speed_rpm,
                        temp_tolerance_min, temp_tolerance_max, pressure_tolerance_min, pressure_tolerance_max,
                        is_mandatory, instruction, created_at)
                    VALUES (
                        @CardId, @StepOrder, @StepName, @StepType, @EquipmentId,
                        @PlannedTempC, @PlannedPressureBar, @PlannedDurationMin, @PlannedSpeedRpm,
                        @TempToleranceMin, @TempToleranceMax, @PressureToleranceMin, @PressureToleranceMax,
                        @IsMandatory, @Instruction, GETDATE())";
                await conn.ExecuteAsync(stepSql, new
                {
                    CardId = cardId,
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
            trans.Commit();
            return cardId;
        }
        catch { trans.Rollback(); throw; }
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
}
