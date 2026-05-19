using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class EquipmentDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? EquipmentType { get; set; }
        public string? LineNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
