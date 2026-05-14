namespace API.Models.DTOs
{
    public class TechStepDto
    {
        public int Id { get; set; }
        public int TechCardId { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string StepType { get; set; } = string.Empty;
        public int? EquipmentId { get; set; }
        public string? EquipmentName { get; set; }
        public decimal? PlannedTempC { get; set; }
        public decimal? PlannedPressureBar { get; set; }
        public int? PlannedDurationMin { get; set; }
        public int? PlannedSpeedRpm { get; set; }
        public decimal? TempToleranceMin { get; set; }
        public decimal? TempToleranceMax { get; set; }
        public decimal? PressureToleranceMin { get; set; }
        public decimal? PressureToleranceMax { get; set; }
        public bool IsMandatory { get; set; } = true;
        public string? Instruction { get; set; }
    }
}
