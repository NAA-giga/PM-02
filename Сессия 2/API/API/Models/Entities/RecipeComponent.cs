using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Models.Entities
{
    [Table("recipe_components")]
    public class RecipeComponent
    {
        [Key]
        public int Id { get; set; }

        [Column("recipe_id")]
        public int RecipeId { get; set; }

        [Column("raw_material_id")]
        public int RawMaterialId { get; set; }

        [Column("percentage")]
        public decimal Percentage { get; set; }

        [Column("load_order")]
        public int LoadOrder { get; set; }

        [Column("tolerance_min")]
        public decimal ToleranceMin { get; set; } = 0;

        [Column("tolerance_max")]
        public decimal ToleranceMax { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
