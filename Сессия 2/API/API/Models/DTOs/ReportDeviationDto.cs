namespace API.Models.DTOs
{
    public class ReportDeviationDto
    {
        public int ProductionBatchId { get; set; }
        public int? StepExecutionId { get; set; }           // ссылка на batch_step_execution.id, если отклонение в шаге
        public string DeviationType { get; set; } = string.Empty; // "parameter", "equipment", "material", "process"
        public string Severity { get; set; } = "warning";   // "info", "warning", "critical"
        public string Description { get; set; } = string.Empty;
        public string? PlannedValue { get; set; }
        public string? ActualValue { get; set; }
        public string? ParameterName { get; set; }
    }
}
