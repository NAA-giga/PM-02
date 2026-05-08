namespace API.Models.DTOs
{
    public class ProductionBatchResponseDto
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal PlannedQuantityKg { get; set; }
        public decimal? ActualQuantityKg { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? LabDecision { get; set; }
        public DateTime? LabDecisionDate { get; set; }
        public string? LabDecisionReason { get; set; }
        public List<BatchStepExecutionDto> Steps { get; set; } = new();
    }
}
