using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("batch_raw_material_usage")]
    public class BatchRawMaterialUsage
    {
        [Key]
        public int Id { get; set; }

        [Column("production_batch_id")]
        public int ProductionBatchId { get; set; }

        [Column("raw_material_batch_id")]
        public int RawMaterialBatchId { get; set; }

        [Column("quantity_used")]
        public decimal QuantityUsed { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
