namespace API.Models.DTOs
{
    public class ChangeOrderStatusDto
    {
        public string Status { get; set; } = string.Empty; // confirmed, cancelled, etc
    }
}
