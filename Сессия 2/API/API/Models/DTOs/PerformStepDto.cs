namespace API.Models.DTOs
{
    public class PerformStepDto
    {
        public decimal? ActualTempC { get; set; }
        public decimal? ActualPressureBar { get; set; }
        public int? ActualDurationMin { get; set; }
        public string? OperatorComment { get; set; }
    }
}
