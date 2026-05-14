// API/Models/Entities/ProductionOrder.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.Entities
{
    [Table("production_orders")]
    public class ProductionOrder
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("order_number")]
        public string OrderNumber { get; set; } = string.Empty;

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("recipe_id")]
        public int RecipeId { get; set; }

        [Column("tech_card_id")]
        public int TechCardId { get; set; }

        [Column("planned_quantity_kg")]
        public decimal PlannedQuantityKg { get; set; }

        [Column("status")]
        public string Status { get; set; } = "draft"; // draft, confirmed, in_progress, completed, cancelled

        [Column("planned_start_date")]
        public DateTime PlannedStartDate { get; set; }

        [Column("actual_start_date")]
        public DateTime? ActualStartDate { get; set; }

        [Column("actual_end_date")]
        public DateTime? ActualEndDate { get; set; }

        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}