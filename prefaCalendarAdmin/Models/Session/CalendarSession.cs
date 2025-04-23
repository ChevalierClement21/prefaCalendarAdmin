using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace prefaCalendarAdmin.Models.Session
{
    public class CalendarSession
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [Column("name")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "L'année est obligatoire")]
        [Column("year")]
        public int Year { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}