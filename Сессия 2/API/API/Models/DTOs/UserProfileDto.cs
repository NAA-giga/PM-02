namespace API.Models.DTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;      // имя роли
        public string Department { get; set; } = string.Empty; // имя отдела
        public string? PhotoBase64 { get; set; }              // фото в base64
    }
}
