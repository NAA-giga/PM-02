using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("production_batches")]
    public class ProductionBatch
    {
        [Key]
        public int Id { get; set; }

        [Column("batch_number")]
        public string BatchNumber { get; set; } = string.Empty;

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("recipe_id")]
        public int RecipeId { get; set; }

        [Column("tech_card_id")]
        public int TechCardId { get; set; }

        [Column("status")]
        public string Status { get; set; } = "created";

        [Column("planned_quantity_kg")]
        public decimal PlannedQuantityKg { get; set; }

        [Column("actual_quantity_kg")]
        public decimal? ActualQuantityKg { get; set; }

        [Column("start_time")]
        public DateTime? StartTime { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [Column("lab_decision")]
        public string? LabDecision { get; set; }

        [Column("lab_decision_date")]
        public DateTime? LabDecisionDate { get; set; }

        [Column("lab_decision_by")]
        public int? LabDecisionBy { get; set; }

        [Column("lab_decision_reason")]
        public string? LabDecisionReason { get; set; }

        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
