using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class ProductionBatchDto
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int RecipeId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal PlannedQuantityKg { get; set; }
        public decimal? ActualQuantityKg { get; set; }
        public DateTime? StartTime { get; set; }
        public string? LabDecision { get; set; }
        public DateTime? LabDecisionDate { get; set; }
        public string? LabDecisionReason { get; set; }
        public string? LabDecisionBy { get; set; }
    }
}
