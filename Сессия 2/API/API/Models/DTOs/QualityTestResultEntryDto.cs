namespace API.Models.DTOs
{
    public class QualityTestResultEntryDto
    {
        public int ResultId { get; set; }               // id записи в quality_test_results (0 – если новая)
        public string ParameterName { get; set; } = string.Empty;
        public decimal? MeasuredValue { get; set; }
        public string? AnalystComment { get; set; }
    }
}
