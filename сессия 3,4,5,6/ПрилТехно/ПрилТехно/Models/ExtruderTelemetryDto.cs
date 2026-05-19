using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class ExtruderTelemetryDto
    {
        public int Id { get; set; }
        public int ProductionBatchId { get; set; }
        public int ZoneNumber { get; set; }
        public decimal? TemperatureC { get; set; }
        public decimal? PressureBar { get; set; }
        public int? ScrewSpeedRpm { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
