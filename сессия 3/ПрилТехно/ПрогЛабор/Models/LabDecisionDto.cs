namespace ПрогЛабор.Models
{
    public class LabDecisionDto
    {
        public int BatchId { get; set; }
        public bool IsRawMaterial { get; set; } // true – сырьё, false – готовая продукция
        public string Decision { get; set; } = string.Empty; // "approved" или "blocked"
        public string? Reason { get; set; } // обязателен для blocked
    }
}