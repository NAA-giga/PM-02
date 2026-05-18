using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class ExtruderReportRow
    {
        public string Партия { get; set; } = string.Empty;
        public int Зона { get; set; }
        public decimal? Температура { get; set; }
        public decimal? Давление { get; set; }
        public int? Скорость { get; set; }
        public DateTime Время { get; set; }
    }
}
