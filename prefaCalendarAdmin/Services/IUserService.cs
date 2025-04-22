using prefaCalendarAdmin.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace prefaCalendarAdmin.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> UpdateUserApprovalStatusAsync(int id, bool approved);
        
        // Nouvelles méthodes pour la gestion des administrateurs
        Task<List<UserWithRoles>> GetAllUsersWithRolesAsync();
        Task<UserWithRoles?> GetUserWithRolesByIdAsync(int id);
        Task<bool> SetAdminStatusAsync(int userId, bool isAdmin);
    }
}
