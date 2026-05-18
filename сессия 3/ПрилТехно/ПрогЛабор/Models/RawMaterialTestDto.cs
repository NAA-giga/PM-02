using System;

namespace ПрогЛабор.Models
{
    public class RawMaterialTestDto
    {
        public int Id { get; set; }
        public int RawMaterialBatchId { get; set; }
        public string TestNumber { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty; // входной контроль, повторный и т.д.
        public string Status { get; set; } = "scheduled"; // scheduled, in_progress, completed
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public string? Decision { get; set; } // approved, blocked (на уровне испытания, опционально)
        public string? DecisionReason { get; set; }
    }
}