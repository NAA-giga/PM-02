using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class DeviationsReportRow
    {
        public string Партия { get; set; } = string.Empty;
        public string Шаг { get; set; } = string.Empty;
        public string Параметр { get; set; } = string.Empty;
        public string План { get; set; } = string.Empty;
        public string Факт { get; set; } = string.Empty;
        public string Тип { get; set; } = string.Empty;
        public string Серьёзность { get; set; } = string.Empty;
        public DateTime Дата { get; set; }
    }
}
