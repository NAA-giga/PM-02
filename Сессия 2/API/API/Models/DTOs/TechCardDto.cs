namespace API.Models.DTOs
{
    public class TechCardDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Version { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<TechStepDto> Steps { get; set; } = new();
    }
}
