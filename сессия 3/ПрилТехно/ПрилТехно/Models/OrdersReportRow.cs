using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class OrdersReportRow
    {
        public string НомерЗаказа { get; set; } = string.Empty;
        public string Продукт { get; set; } = string.Empty;
        public decimal ПланКг { get; set; }
        public string Статус { get; set; } = string.Empty;
        public DateTime ПлановаяДата { get; set; }
        public DateTime? ФактНачало { get; set; }
        public DateTime? ФактКонец { get; set; }
    }
}
