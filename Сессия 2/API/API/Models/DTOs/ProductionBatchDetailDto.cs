namespace API.Models.DTOs
{
    public class ProductionBatchDetailDto
    {
        public List<BatchStepExecutionDto> Steps { get; set; } = new();
    }
}
