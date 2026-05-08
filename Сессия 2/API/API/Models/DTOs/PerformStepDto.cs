namespace API.Models.DTOs
{
    public class PerformStepDto
    {
        public int BatchId { get; set; }
        public int StepOrder { get; set; }
        public decimal? ActualTempC { get; set; }
        public decimal? ActualPressureBar { get; set; }
        public int? ActualDurationMin { get; set; }
        public string? OperatorComment { get; set; }
    }
}
