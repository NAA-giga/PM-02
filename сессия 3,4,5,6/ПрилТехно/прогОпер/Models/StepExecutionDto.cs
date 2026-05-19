namespace прогОпер.Models
{
    public class StepExecutionDto
    {
        public int Id { get; set; }                  // batch_step_execution.id (0 если ещё не создана)
        public int StepId { get; set; }              // tech_steps.id
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
        public bool IsMandatory { get; set; }
        public string? Instruction { get; set; }
        public string Status { get; set; } = "pending";  // pending, running, completed
        public decimal? ActualTempC { get; set; }
        public decimal? ActualPressureBar { get; set; }
        public int? ActualDurationMin { get; set; }
        public int? ActualSpeedRpm { get; set; }
        public bool DeviationFlag { get; set; }
        public string? DeviationDescription { get; set; }
        public string? OperatorComment { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}