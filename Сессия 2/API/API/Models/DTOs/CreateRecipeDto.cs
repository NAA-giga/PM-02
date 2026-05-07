namespace API.Models.DTOs;

public class CreateRecipeDto
{
    public int ProductId { get; set; }
    public int Version { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<RecipeComponentDto> Components { get; set; } = new();
}