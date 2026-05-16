namespace API.Models.DTOs
{
    public class DeviationDto
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public int? StepOrder { get; set; }
        public string? StepName { get; set; }
        public string DeviationType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? PlannedValue { get; set; }
        public string? ActualValue { get; set; }
        public string? ParameterName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
