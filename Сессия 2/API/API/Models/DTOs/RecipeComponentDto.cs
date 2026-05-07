namespace API.Models.DTOs
{
    public class RecipeComponentDto
    {
        public int? Id { get; set; }          // null для новых компонентов
        public int RawMaterialId { get; set; }
        public decimal Percentage { get; set; }
        public int LoadOrder { get; set; }
        public decimal? ToleranceMin { get; set; }
        public decimal? ToleranceMax { get; set; }
    }
}
