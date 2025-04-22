using System.Collections.Generic;

namespace prefaCalendarAdmin.Models
{
    public class UserWithRoles : User
    {
        public List<Role> Roles { get; set; } = new List<Role>();
        public bool IsAdmin => Roles.Exists(r => r.Name == "admin");
    }
}
