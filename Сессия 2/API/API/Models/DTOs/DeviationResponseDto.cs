namespace API.Models.DTOs
{
    public class DeviationResponseDto
    {
        public int Id { get; set; }
        public int ProductionBatchId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public int? StepExecutionId { get; set; }
        public int? StepOrder { get; set; }
        public string DeviationType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? PlannedValue { get; set; }
        public string? ActualValue { get; set; }
        public string? ParameterName { get; set; }
        public string ResolutionStatus { get; set; } = string.Empty;
        public string? ResolutionComment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
