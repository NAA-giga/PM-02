namespace API.Models.DTOs;
public class RecipeComponentResponseDto
{
    public int Id { get; set; }
    public int RawMaterialId { get; set; }
    public string RawMaterialName { get; set; } = string.Empty;
    public string RawMaterialCode { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public int LoadOrder { get; set; }
    public decimal? ToleranceMin { get; set; }
    public decimal? ToleranceMax { get; set; }
}
