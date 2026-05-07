namespace API.Models.DTOs;

public class UpdateRecipeDto
{
    public string Name { get; set; } = string.Empty;
    public List<RecipeComponentDto> Components { get; set; } = new();
}