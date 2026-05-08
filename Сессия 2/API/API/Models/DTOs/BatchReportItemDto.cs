namespace API.Models.DTOs
{
    public class BatchReportItemDto
    {
        public string BatchNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal PlannedQuantityKg { get; set; }
        public decimal? ActualQuantityKg { get; set; }
        public decimal? YieldPercent { get; set; }
        public bool HasDeviations { get; set; }
        public string? LabDecision { get; set; }
    }
}
