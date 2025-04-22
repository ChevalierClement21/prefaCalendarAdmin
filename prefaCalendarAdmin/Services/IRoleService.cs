using prefaCalendarAdmin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace prefaCalendarAdmin.Services
{
    public interface IRoleService
    {
        Task<List<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByNameAsync(string name);
        Task<List<Role>> GetUserRolesAsync(int userId);
        Task<bool> AssignRoleToUserAsync(int userId, string roleName);
        Task<bool> RemoveRoleFromUserAsync(int userId, string roleName);
    }
}
