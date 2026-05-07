using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("quality_test_results")]
    public class QualityTestResult
    {
        [Key]
        public int Id { get; set; }

        [Column("test_id")]
        public int TestId { get; set; }

        [Column("parameter_name")]
        public string ParameterName { get; set; } = string.Empty;

        [Column("measured_value")]
        public decimal? MeasuredValue { get; set; }

        [Column("standard_value_min")]
        public decimal? StandardValueMin { get; set; }

        [Column("standard_value_max")]
        public decimal? StandardValueMax { get; set; }

        [Column("standard_text")]
        public string? StandardText { get; set; }

        [Column("unit")]
        public string? Unit { get; set; }

        [Column("result")]
        public string? Result { get; set; }

        [Column("is_critical")]
        public bool IsCritical { get; set; } = false;

        [Column("analyst_comment")]
        public string? AnalystComment { get; set; }

        [Column("measured_at")]
        public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
    }
}
