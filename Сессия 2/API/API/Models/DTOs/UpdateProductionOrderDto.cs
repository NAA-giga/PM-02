namespace API.Models.DTOs
{
    public class UpdateProductionOrderDto
    {
        public string? OrderNumber { get; set; }
        public decimal? PlannedQuantityKg { get; set; }
        public DateTime? PlannedStartDate { get; set; }
    }
}
