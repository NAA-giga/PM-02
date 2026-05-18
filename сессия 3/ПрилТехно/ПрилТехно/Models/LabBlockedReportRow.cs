using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class LabBlockedReportRow
    {
        public string Партия { get; set; } = string.Empty;
        public string Продукт { get; set; } = string.Empty;
        public DateTime? ДатаБлокировки { get; set; }
        public string Причина { get; set; } = string.Empty;
        public string Ответственный { get; set; } = string.Empty;
    }
}
