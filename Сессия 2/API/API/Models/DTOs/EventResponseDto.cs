namespace API.Models.DTOs
{
    public class EventResponseDto
    {
        public int Id { get; set; }
        public string EventType { get; set; } = string.Empty;   // "deviation", "batch_started", "batch_completed", "lab_decision"
        public string SourceType { get; set; } = string.Empty; // "batch", "test", "deviation"
        public int SourceId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
