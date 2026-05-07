using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("tech_steps")]
    public class TechStep
    {
        [Key]
        public int Id { get; set; }

        [Column("tech_card_id")]
        public int TechCardId { get; set; }

        [Column("step_order")]
        public int StepOrder { get; set; }

        [Column("step_name")]
        public string StepName { get; set; } = string.Empty;

        [Column("step_type")]
        public string StepType { get; set; } = string.Empty;

        [Column("equipment_id")]
        public int? EquipmentId { get; set; }

        [Column("planned_temp_c")]
        public decimal? PlannedTempC { get; set; }

        [Column("planned_pressure_bar")]
        public decimal? PlannedPressureBar { get; set; }

        [Column("planned_duration_min")]
        public int? PlannedDurationMin { get; set; }

        [Column("planned_speed_rpm")]
        public int? PlannedSpeedRpm { get; set; }

        [Column("temp_tolerance_min")]
        public decimal? TempToleranceMin { get; set; }

        [Column("temp_tolerance_max")]
        public decimal? TempToleranceMax { get; set; }

        [Column("pressure_tolerance_min")]
        public decimal? PressureToleranceMin { get; set; }

        [Column("pressure_tolerance_max")]
        public decimal? PressureToleranceMax { get; set; }

        [Column("is_mandatory")]
        public bool IsMandatory { get; set; } = true;

        [Column("instruction")]
        public string? Instruction { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
