using System;

namespace ПрогЛабор.Models
{
    public class RawMaterialTestResultDto
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public decimal? MeasuredValue { get; set; }
        public decimal? StandardValueMin { get; set; }
        public decimal? StandardValueMax { get; set; }
        public string? StandardText { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty; // pass, fail, not_tested
        public bool IsCritical { get; set; }
        public string? AnalystComment { get; set; }
        public DateTime MeasuredAt { get; set; }
    }
}