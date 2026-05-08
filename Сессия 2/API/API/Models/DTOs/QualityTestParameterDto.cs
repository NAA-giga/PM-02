namespace API.Models.DTOs
{
    public class QualityTestParameterDto
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal? StandardValueMin { get; set; }
        public decimal? StandardValueMax { get; set; }
        public string? StandardText { get; set; }       // для текстовых норм, например "отсутствие примесей"
        public string Unit { get; set; } = string.Empty;
        public bool IsCritical { get; set; }            // критический параметр – при несоответствии автоматическая блокировка
    }
}
