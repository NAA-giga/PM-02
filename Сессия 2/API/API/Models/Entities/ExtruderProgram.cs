using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("extruder_programs")]
    public class ExtruderProgram
    {
        [Key]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("version")]
        public int Version { get; set; }

        [Column("production_batch_id")]
        public int? ProductionBatchId { get; set; }

        [Column("zone_params")]
        public string? ZoneParams { get; set; }   // JSON строка

        [Column("status")]
        public string Status { get; set; } = "draft";

        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
