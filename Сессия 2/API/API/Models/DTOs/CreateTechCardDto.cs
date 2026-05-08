namespace API.Models.DTOs;

/// <summary>
/// DTO для создания новой технологической карты
/// </summary>
public class CreateTechCardDto
{
    public int ProductId { get; set; }
    public int Version { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateTechStepDto> Steps { get; set; } = new();
}
