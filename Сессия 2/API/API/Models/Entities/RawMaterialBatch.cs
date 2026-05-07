using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("raw_material_batches")]
    public class RawMaterialBatch
    {
        [Key]
        public int Id { get; set; }

        [Column("batch_number")]
        public string BatchNumber { get; set; } = string.Empty;

        [Column("raw_material_id")]
        public int RawMaterialId { get; set; }

        [Column("supplier_batch_number")]
        public string? SupplierBatchNumber { get; set; }

        [Column("supplier_name")]
        public string? SupplierName { get; set; }

        [Column("quantity")]
        public decimal Quantity { get; set; }

        [Column("unit")]
        public string Unit { get; set; } = "kg";

        [Column("receipt_date")]
        public DateTime ReceiptDate { get; set; }

        [Column("expiration_date")]
        public DateTime? ExpirationDate { get; set; }

        [Column("lab_status")]
        public string LabStatus { get; set; } = "pending";

        [Column("storage_location")]
        public string? StorageLocation { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
