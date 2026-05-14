namespace API.Models.DTOs
{
    public class ZoneParams
    {
        public decimal? TemperatureC { get; set; }
        public decimal? PressureBar { get; set; }
        public int? ScrewSpeedRpm { get; set; }
    }
}
