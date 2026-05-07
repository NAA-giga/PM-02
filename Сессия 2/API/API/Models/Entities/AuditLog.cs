using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("audit_log")]
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Column("table_name")]
        public string TableName { get; set; } = string.Empty;

        [Column("record_id")]
        public int RecordId { get; set; }

        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("old_value")]
        public string? OldValue { get; set; }

        [Column("new_value")]
        public string? NewValue { get; set; }

        [Column("changed_by")]
        public int ChangedBy { get; set; }

        [Column("changed_at")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
