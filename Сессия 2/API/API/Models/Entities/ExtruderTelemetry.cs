using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("extruder_telemetry")]
    public class ExtruderTelemetry
    {
        [Key]
        public int Id { get; set; }

        [Column("production_batch_id")]
        public int ProductionBatchId { get; set; }

        [Column("zone_number")]
        public int ZoneNumber { get; set; }

        [Column("temperature_c")]
        public decimal? TemperatureC { get; set; }

        [Column("pressure_bar")]
        public decimal? PressureBar { get; set; }

        [Column("screw_speed_rpm")]
        public int? ScrewSpeedRpm { get; set; }

        [Column("recorded_at")]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
