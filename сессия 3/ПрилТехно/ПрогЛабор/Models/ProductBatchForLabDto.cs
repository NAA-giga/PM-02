using System;

namespace ПрогЛабор.Models
{
    public class ProductBatchForLabDto
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal PlannedQuantityKg { get; set; }
        public decimal? ActualQuantityKg { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty; // quality_control, completed, blocked
        public string? LabDecision { get; set; } // approved, blocked
        public DateTime? LabDecisionDate { get; set; }
        public string? LabDecisionReason { get; set; }
        public int? LastTestId { get; set; }
        public DateTime? LastTestDate { get; set; }
    }
}