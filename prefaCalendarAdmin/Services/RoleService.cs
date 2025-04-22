using Dapper;
using MySqlConnector;
using prefaCalendarAdmin.Config;
using prefaCalendarAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace prefaCalendarAdmin.Services
{
    public class RoleService : IRoleService
    {
        private readonly DatabaseConfig _dbConfig;

        public RoleService(DatabaseConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            var roles = await connection.QueryAsync<Role>("SELECT * FROM roles");
            return roles.AsList();
        }

        public async Task<Role?> GetRoleByNameAsync(string name)
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            return await connection.QueryFirstOrDefaultAsync<Role>(
                "SELECT * FROM roles WHERE name = @Name", 
                new { Name = name });
        }

        public async Task<List<Role>> GetUserRolesAsync(int userId)
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            var roles = await connection.QueryAsync<Role>(@"
                SELECT r.* 
                FROM roles r
                JOIN assigned_roles ar ON r.id = ar.role_id
                WHERE ar.entity_id = @UserId", 
                new { UserId = userId });
                
            return roles.AsList();
        }

        public async Task<bool> AssignRoleToUserAsync(int userId, string roleName)
        {
            Console.WriteLine($"AssignRoleToUserAsync - UserID: {userId}, RoleName: {roleName}");
            
            try
            {
                using var connection = new MySqlConnection(_dbConfig.ConnectionString);
                Console.WriteLine("Ouverture de la connexion à la base de données");
                await connection.OpenAsync();
                
                // Vérifier si le rôle existe
                Console.WriteLine($"Recherche du rôle '{roleName}'");
                var role = await GetRoleByNameAsync(roleName);
                
                if (role == null)
                {
                    Console.WriteLine($"Rôle '{roleName}' non trouvé dans la base de données, tentative de création");
                    
                    // Créer le rôle s'il n'existe pas
                    var insertRoleResult = await connection.ExecuteAsync(@"
                        INSERT INTO roles (name, title, created_at, updated_at)
                        VALUES (@Name, @Title, @Now, @Now)",
                        new { 
                            Name = roleName, 
                            Title = roleName.First().ToString().ToUpper() + roleName.Substring(1), // Capitaliser le titre
                            Now = DateTime.UtcNow
                        });
                        
                    Console.WriteLine($"Résultat de la création du rôle: {insertRoleResult}");
                    
                    if (insertRoleResult > 0)
                    {
                        // Récupérer le rôle nouvellement créé
                        role = await GetRoleByNameAsync(roleName);
                    }
                    
                    if (role == null)
                    {
                        Console.WriteLine("Impossible de créer ou de récupérer le rôle");
                        return false;
                    }
                }
                
                Console.WriteLine($"Rôle '{roleName}' trouvé avec ID: {role.Id}");
                
                using var transaction = await connection.BeginTransactionAsync();
                Console.WriteLine("Transaction démarrée");
                
                try
                {
                    // Vérifier si l'utilisateur a déjà ce rôle
                    Console.WriteLine("Vérification si l'utilisateur a déjà ce rôle");
                    var existingAssignment = await connection.QueryFirstOrDefaultAsync<int>(@"
                        SELECT COUNT(*) FROM assigned_roles 
                        WHERE role_id = @RoleId AND entity_id = @UserId",
                        new { RoleId = role.Id, UserId = userId },
                        transaction);
                    
                    Console.WriteLine($"Nombre d'attributions existantes: {existingAssignment}");
                    
                    // Si l'utilisateur n'a pas encore ce rôle, l'ajouter
                    if (existingAssignment == 0)
                    {
                        Console.WriteLine("Ajout du rôle à l'utilisateur");
                        var result = await connection.ExecuteAsync(@"
                            INSERT INTO assigned_roles (role_id, entity_id, entity_type)
                            VALUES (@RoleId, @UserId, 'App\\Models\\User')",
                            new { 
                                RoleId = role.Id, 
                                UserId = userId
                            },
                            transaction);
                            
                        Console.WriteLine($"Résultat de l'insertion: {result} ligne(s) affectée(s)");
                    }
                    else
                    {
                        Console.WriteLine("L'utilisateur a déjà ce rôle, aucune action nécessaire");
                    }
                    
                    Console.WriteLine("Validation de la transaction");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction validée avec succès");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'attribution du rôle: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    Console.WriteLine("Annulation de la transaction");
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de connexion à la base de données: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, string roleName)
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            // Vérifier si le rôle existe
            var role = await GetRoleByNameAsync(roleName);
            if (role == null)
                return false;
                
            // Supprimer l'attribution du rôle
            var result = await connection.ExecuteAsync(@"
                DELETE FROM assigned_roles 
                WHERE role_id = @RoleId AND entity_id = @UserId",
                new { RoleId = role.Id, UserId = userId });
                
            return result > 0;
        }
    }
}
