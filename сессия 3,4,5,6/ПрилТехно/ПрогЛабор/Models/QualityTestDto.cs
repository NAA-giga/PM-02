using System;

namespace ПрогЛабор.Models
{
    public class QualityTestDto
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public string TestNumber { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public string Status { get; set; } = "scheduled";
        public string Priority { get; set; } = "normal";
        public DateTime CreatedDate { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public string? Decision { get; set; } // опционально
        public string? DecisionReason { get; set; }
    }
}