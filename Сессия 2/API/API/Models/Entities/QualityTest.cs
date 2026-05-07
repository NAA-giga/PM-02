using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("quality_tests")]
    public class QualityTest
    {
        [Key]
        public int Id { get; set; }

        [Column("test_number")]
        public string TestNumber { get; set; } = string.Empty;

        [Column("batch_id")]
        public int BatchId { get; set; }

        [Column("test_type")]
        public string TestType { get; set; } = string.Empty;

        [Column("status")]
        public string Status { get; set; } = "scheduled";

        [Column("priority")]
        public string Priority { get; set; } = "normal";

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("scheduled_date")]
        public DateTime ScheduledDate { get; set; }

        [Column("completed_date")]
        public DateTime? CompletedDate { get; set; }

        [Column("assigned_to")]
        public int? AssignedTo { get; set; }

        [Column("created_by")]
        public int CreatedBy { get; set; }
    }
}
