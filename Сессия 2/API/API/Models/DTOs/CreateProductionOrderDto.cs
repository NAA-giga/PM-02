namespace API.Models.DTOs
{
    public class CreateProductionOrderDto
    {
        public string OrderNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int RecipeId { get; set; }
        public int TechCardId { get; set; }
        public decimal PlannedQuantityKg { get; set; }
        public DateTime PlannedStartDate { get; set; }
    }
}
