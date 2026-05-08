namespace API.Models.DTOs
{
public class CreateQualityTestDto
{
    public int BatchId { get; set; }               // production_batch.id
    public string TestType { get; set; } = string.Empty; // "finished_product" or "raw_material"
    public DateTime ScheduledDate { get; set; }
    public int? AssignedTo { get; set; }           // user id (лаборант)
    public List<QualityTestParameterDto> Parameters { get; set; } = new();
}
}
