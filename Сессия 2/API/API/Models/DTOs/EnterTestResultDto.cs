namespace API.Models.DTOs
{
    public class EnterTestResultDto
    {
        public int TestId { get; set; }
        public List<QualityTestResultEntryDto> Results { get; set; } = new();
    }
}
