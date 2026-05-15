namespace API.Models.DTOs
{
    public class CreateEventDto
    {
        public string EventType { get; set; } = string.Empty;
        public string SourceType { get; set; } = string.Empty;
        public int SourceId { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; }
    }
}
