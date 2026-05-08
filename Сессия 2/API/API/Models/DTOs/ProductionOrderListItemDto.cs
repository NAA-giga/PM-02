namespace API.Models.DTOs
{
    public class ProductionOrderListItemDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal PlannedQuantityKg { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime PlannedStartDate { get; set; }
    }
}
