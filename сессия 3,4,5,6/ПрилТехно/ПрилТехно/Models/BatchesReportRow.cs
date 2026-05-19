using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class BatchesReportRow
    {
        public string НомерПартии { get; set; } = string.Empty;
        public string Продукт { get; set; } = string.Empty;
        public DateTime? ДатаЗапуска { get; set; }
        public string Статус { get; set; } = string.Empty;
        public decimal ПланКг { get; set; }
        public decimal ФактКг { get; set; }
        public string ЛабРешение { get; set; } = string.Empty;
    }
}
