namespace API.Models.DTOs
{
    public class BatchStepExecutionDto
    {
        public int StepOrder { get; set; }
        public string StepName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // pending, running, completed, skipped
        public decimal? ActualTempC { get; set; }
        public decimal? ActualPressureBar { get; set; }
        public int? ActualDurationMin { get; set; }
        public bool DeviationFlag { get; set; }
        public string? DeviationDescription { get; set; }
        public string? OperatorComment { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
