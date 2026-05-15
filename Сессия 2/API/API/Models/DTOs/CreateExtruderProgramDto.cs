namespace API.Models.DTOs
{
    public class CreateExtruderProgramDto
    {
        public string Name { get; set; } = string.Empty;
        public int Version { get; set; }
        public int? ProductionBatchId { get; set; }
        public Dictionary<int, ZoneParams> ZoneParameters { get; set; } = new();
        public string Status { get; set; } = "draft";
    }
}
