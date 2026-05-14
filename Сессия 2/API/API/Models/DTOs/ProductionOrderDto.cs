namespace API.Models.DTOs
{
    public class ProductionOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int RecipeId { get; set; }
        public string RecipeName { get; set; } = string.Empty;
        public int TechCardId { get; set; }
        public string TechCardName { get; set; } = string.Empty;
        public decimal PlannedQuantityKg { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime PlannedStartDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
