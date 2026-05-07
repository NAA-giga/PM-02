using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("deviations")]
    public class Deviation
    {
        [Key]
        public int Id { get; set; }

        [Column("production_batch_id")]
        public int ProductionBatchId { get; set; }

        [Column("step_execution_id")]
        public int? StepExecutionId { get; set; }

        [Column("deviation_type")]
        public string DeviationType { get; set; } = string.Empty;

        [Column("severity")]
        public string Severity { get; set; } = "warning";

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("planned_value")]
        public string? PlannedValue { get; set; }

        [Column("actual_value")]
        public string? ActualValue { get; set; }

        [Column("parameter_name")]
        public string? ParameterName { get; set; }

        [Column("resolution_status")]
        public string ResolutionStatus { get; set; } = "new";

        [Column("resolution_comment")]
        public string? ResolutionComment { get; set; }

        [Column("resolved_by")]
        public int? ResolvedBy { get; set; }

        [Column("resolved_at")]
        public DateTime? ResolvedAt { get; set; }

        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
