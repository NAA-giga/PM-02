namespace API.Models.DTOs;

public class RecipeResponseDto : RecipeDto
{
    public string? ApprovedByName { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public new List<RecipeComponentResponseDto> Components { get; set; } = new();
}

