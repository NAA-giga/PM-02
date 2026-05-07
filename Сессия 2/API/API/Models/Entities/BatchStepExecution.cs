using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("batch_step_execution")]
    public class BatchStepExecution
    {
        [Key]
        public int Id { get; set; }

        [Column("production_batch_id")]
        public int ProductionBatchId { get; set; }

        [Column("step_id")]
        public int StepId { get; set; }

        [Column("step_order")]
        public int StepOrder { get; set; }

        [Column("step_name")]
        public string StepName { get; set; } = string.Empty;

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("actual_temp_c")]
        public decimal? ActualTempC { get; set; }

        [Column("actual_pressure_bar")]
        public decimal? ActualPressureBar { get; set; }

        [Column("actual_duration_min")]
        public int? ActualDurationMin { get; set; }

        [Column("actual_speed_rpm")]
        public int? ActualSpeedRpm { get; set; }

        [Column("deviation_flag")]
        public bool DeviationFlag { get; set; } = false;

        [Column("deviation_description")]
        public string? DeviationDescription { get; set; }

        [Column("start_time")]
        public DateTime? StartTime { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [Column("started_by")]
        public int? StartedBy { get; set; }

        [Column("completed_by")]
        public int? CompletedBy { get; set; }

        [Column("operator_comment")]
        public string? OperatorComment { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
