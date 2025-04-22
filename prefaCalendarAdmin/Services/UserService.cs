using Dapper;
using MySqlConnector;
using prefaCalendarAdmin.Config;
using prefaCalendarAdmin.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace prefaCalendarAdmin.Services
{
    public class UserService : IUserService
    {
        private readonly DatabaseConfig _dbConfig;
        private readonly IRoleService _roleService;

        public UserService(DatabaseConfig dbConfig, IRoleService roleService)
        {
            _dbConfig = dbConfig;
            _roleService = roleService;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            var users = await connection.QueryAsync<User>("SELECT * FROM users");
            return users.AsList();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM users WHERE id = @Id", 
                new { Id = id });
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM users WHERE email = @Email", 
                new { Email = email });
        }

        public async Task<bool> UpdateUserApprovalStatusAsync(int id, bool approved)
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            var result = await connection.ExecuteAsync(
                "UPDATE users SET approved = @Approved, updated_at = @UpdatedAt WHERE id = @Id",
                new { 
                    Id = id, 
                    Approved = approved,
                    UpdatedAt = DateTime.UtcNow
                });
                
            return result > 0;
        }
        
        public async Task<List<UserWithRoles>> GetAllUsersWithRolesAsync()
        {
            var users = await GetAllUsersAsync();
            var usersWithRoles = new List<UserWithRoles>();
            
            foreach (var user in users)
            {
                var userWithRoles = new UserWithRoles
                {
                    Id = user.Id,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    Email = user.Email,
                    EmailVerifiedAt = user.EmailVerifiedAt,
                    Approved = user.Approved,
                    Password = user.Password,
                    RememberToken = user.RememberToken,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Roles = await _roleService.GetUserRolesAsync(user.Id)
                };
                
                usersWithRoles.Add(userWithRoles);
            }
            
            return usersWithRoles;
        }
        
        public async Task<UserWithRoles?> GetUserWithRolesByIdAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            
            if (user == null)
                return null;
                
            var roles = await _roleService.GetUserRolesAsync(id);
            
            return new UserWithRoles
            {
                Id = user.Id,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Email = user.Email,
                EmailVerifiedAt = user.EmailVerifiedAt,
                Approved = user.Approved,
                Password = user.Password,
                RememberToken = user.RememberToken,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = roles
            };
        }
        
        public async Task<bool> SetAdminStatusAsync(int userId, bool isAdmin)
        {
            const string adminRoleName = "admin";
            
            Console.WriteLine($"SetAdminStatusAsync - UserID: {userId}, isAdmin: {isAdmin}");
            
            try
            {
                if (isAdmin)
                {
                    // Récupérer l'utilisateur pour vérifier s'il existe
                    var user = await GetUserByIdAsync(userId);
                    if (user == null)
                    {
                        Console.WriteLine($"Utilisateur avec ID {userId} non trouvé");
                        return false;
                    }
                    
                    Console.WriteLine($"Approbation de l'utilisateur ID: {userId}");
                    // S'assurer que l'utilisateur est approuvé avant de devenir admin
                    bool approvalResult = await UpdateUserApprovalStatusAsync(userId, true);
                    Console.WriteLine($"Résultat de l'approbation: {approvalResult}");
                    
                    Console.WriteLine($"Attribution du rôle admin à l'utilisateur ID: {userId}");
                    bool assignResult = await _roleService.AssignRoleToUserAsync(userId, adminRoleName);
                    Console.WriteLine($"Résultat de l'attribution du rôle: {assignResult}");
                    
                    return assignResult;
                }
                else
                {
                    Console.WriteLine($"Retrait du rôle admin pour l'utilisateur ID: {userId}");
                    bool removeResult = await _roleService.RemoveRoleFromUserAsync(userId, adminRoleName);
                    Console.WriteLine($"Résultat du retrait du rôle: {removeResult}");
                    
                    return removeResult;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur dans SetAdminStatusAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
