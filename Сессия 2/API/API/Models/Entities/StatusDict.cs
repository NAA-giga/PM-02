using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("status_dict")]
    public class StatusDict
    {
        [Key]
        public int Id { get; set; }

        [Column("entity_type")]
        public string EntityType { get; set; } = string.Empty;

        [Column("status_code")]
        public string StatusCode { get; set; } = string.Empty;

        [Column("status_name")]
        public string StatusName { get; set; } = string.Empty;

        [Column("sort_order")]
        public int? SortOrder { get; set; }
    }
}
