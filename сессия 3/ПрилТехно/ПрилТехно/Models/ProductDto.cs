using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public string FormType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "active" или "archived"
    }
}
