namespace API.Models.DTOs
{
    public class LaboratoryDecisionDto
    {
        public int BatchId { get; set; }
        public string Decision { get; set; } = string.Empty; // "approved" or "blocked"
        public string? Reason { get; set; }                 // обязателен при блокировке
    }
}
