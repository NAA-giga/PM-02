using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class SystemEventsReportRow
    {
        public string Тип { get; set; } = string.Empty;
        public string Источник { get; set; } = string.Empty;
        public string Сообщение { get; set; } = string.Empty;
        public DateTime Дата { get; set; }
        public string Пользователь { get; set; } = string.Empty;
    }
}
