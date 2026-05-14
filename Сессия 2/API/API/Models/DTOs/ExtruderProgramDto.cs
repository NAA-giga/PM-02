namespace API.Models.DTOs
{
    public class ExtruderProgramDto
    {
        public int? Id { get; set; }                 // null для создания
        public string Name { get; set; } = string.Empty;
        public int Version { get; set; }
        public int? ProductionBatchId { get; set; }  // можно привязать к партии
        public Dictionary<int, ZoneParams> ZoneParameters { get; set; } = new();
        public string Status { get; set; } = "draft";
    }
}
