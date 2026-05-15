using System.Text.Json.Serialization;

namespace API.Models.DTOs
{
    public class ExtruderProgramDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Version { get; set; }
        public int? ProductionBatchId { get; set; }
        public Dictionary<int, ZoneParams> ZoneParameters { get; set; } = new();
        public string Status { get; set; } = "draft";
        public DateTime CreatedAt { get; set; }
    }


}