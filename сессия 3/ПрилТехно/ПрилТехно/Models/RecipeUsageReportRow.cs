using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class RecipeUsageReportRow
    {
        public string Продукт { get; set; } = string.Empty;
        public string Рецептура { get; set; } = string.Empty;
        public int Версия { get; set; }
        public int КолвоПартий { get; set; }
        public decimal ОбъемКг { get; set; }
    }
}
