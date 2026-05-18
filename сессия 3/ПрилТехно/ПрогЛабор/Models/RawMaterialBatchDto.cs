using System;

namespace ПрогЛабор.Models
{
    public class RawMaterialBatchDto
    {
        public int Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public string RawMaterialName { get; set; } = string.Empty;
        public string? SupplierBatchNumber { get; set; }
        public string? SupplierName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "kg";
        public DateTime ReceiptDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string LabStatus { get; set; } = "pending"; // pending, in_progress, approved, blocked
        public string? StorageLocation { get; set; }
        public int? LastTestId { get; set; }
        public DateTime? LastTestDate { get; set; }
    }
}