namespace API.Models.DTOs
{
    public class EventDto
    {
        public int Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public int SourceId { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
