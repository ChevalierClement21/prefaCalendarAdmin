using System;

namespace prefaCalendarAdmin.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? EmailVerifiedAt { get; set; }
        public bool Approved { get; set; }
        public string Password { get; set; } = string.Empty; // Hashed password from Laravel
        public string RememberToken { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
