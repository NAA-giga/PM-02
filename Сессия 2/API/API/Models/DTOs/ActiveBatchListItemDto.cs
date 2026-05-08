namespace API.Models.DTOs
{
    public class ActiveBatchListItemDto
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public int? CurrentStepOrder { get; set; }
        public string? CurrentStepName { get; set; }
    }
}
