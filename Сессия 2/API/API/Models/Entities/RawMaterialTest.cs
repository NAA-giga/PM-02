using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("raw_material_tests")]
    public class RawMaterialTest
    {
        [Key]
        public int Id { get; set; }

        [Column("test_number")]
        public string TestNumber { get; set; } = string.Empty;

        [Column("raw_material_batch_id")]
        public int RawMaterialBatchId { get; set; }

        [Column("test_type")]
        public string TestType { get; set; } = string.Empty;

        [Column("status")]
        public string Status { get; set; } = "scheduled";

        [Column("decision")]
        public string? Decision { get; set; }

        [Column("decision_reason")]
        public string? DecisionReason { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("completed_date")]
        public DateTime? CompletedDate { get; set; }

        [Column("assigned_to")]
        public int? AssignedTo { get; set; }

        [Column("created_by")]
        public int CreatedBy { get; set; }
    }
}
