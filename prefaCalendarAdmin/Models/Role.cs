using System;

namespace prefaCalendarAdmin.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Level { get; set; }
        public int? Scope { get; set; }
        public int? EntityId { get; set; }
        public string? EntityType { get; set; }
        public bool? Only { get; set; }
        public bool? Except { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
