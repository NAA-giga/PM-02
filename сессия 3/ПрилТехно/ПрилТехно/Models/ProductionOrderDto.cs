using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class ProductionOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal PlannedQuantityKg { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime PlannedStartDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
    }
}
