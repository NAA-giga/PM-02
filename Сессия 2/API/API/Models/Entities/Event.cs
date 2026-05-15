using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.Entities
{
    [Table("events")]
    public class Event
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("event_type")]
        public string EventType { get; set; } = string.Empty;  // batch_started, batch_completed, deviation, lab_decision, etc.

        [Column("source_type")]
        public string SourceType { get; set; } = string.Empty; // batch, test, deviation, extruder

        [Column("source_id")]
        public int SourceId { get; set; }

        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}