using System;
using System.Collections.Generic;
using System.Text;

namespace ПрилТехно.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string? PhotoBase64 { get; set; }
    }
}