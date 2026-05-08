namespace API.Models.DTOs
{
    public class StartProductionBatchDto
    {
        public int OrderId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public decimal PlannedQuantityKg { get; set; }
    }
}
