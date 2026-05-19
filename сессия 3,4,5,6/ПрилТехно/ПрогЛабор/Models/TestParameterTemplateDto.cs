using System.Collections.Generic;

namespace ПрогЛабор.Models
{
    public class TestParameterTemplateDto
    {
        public string ParameterName { get; set; } = string.Empty;
        public decimal? StandardValueMin { get; set; }
        public decimal? StandardValueMax { get; set; }
        public string? StandardText { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsCritical { get; set; }
    }

    public class TestTemplateDto
    {
        public string TestType { get; set; } = string.Empty;
        public List<TestParameterTemplateDto> Parameters { get; set; } = new();
    }
}