using Dapper;
using MySqlConnector;
using prefaCalendarAdmin.Config;
using prefaCalendarAdmin.Models.Session;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace prefaCalendarAdmin.Services
{
    public class SessionService : ISessionService
    {
        private readonly DatabaseConfig _dbConfig;

        public SessionService(DatabaseConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<List<CalendarSession>> GetAllSessionsAsync()
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            var sessions = await connection.QueryAsync<CalendarSession>("SELECT id, name, year, is_active AS IsActive, created_at AS CreatedAt, updated_at AS UpdatedAt FROM calendar_sessions ORDER BY year DESC, name");
            return sessions.ToList();
        }

        public async Task<CalendarSession> GetSessionByIdAsync(int id)
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            return await connection.QueryFirstOrDefaultAsync<CalendarSession>(
                "SELECT id, name, year, is_active AS IsActive, created_at AS CreatedAt, updated_at AS UpdatedAt FROM calendar_sessions WHERE id = @Id", 
                new { Id = id });
        }

        public async Task<CalendarSession> GetActiveSessionAsync()
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            return await connection.QueryFirstOrDefaultAsync<CalendarSession>(
                "SELECT id, name, year, is_active AS IsActive, created_at AS CreatedAt, updated_at AS UpdatedAt FROM calendar_sessions WHERE is_active = 1");
        }

        public async Task<bool> CreateSessionAsync(CalendarSession session)
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            // Si cette session est active, désactiver toutes les autres
            if (session.IsActive)
            {
                await connection.ExecuteAsync(
                    "UPDATE calendar_sessions SET is_active = 0, updated_at = @Now WHERE is_active = 1",
                    new { Now = DateTime.UtcNow });
            }
            
            var now = DateTime.UtcNow;
            var result = await connection.ExecuteAsync(
                @"INSERT INTO calendar_sessions (name, year, is_active, created_at, updated_at) 
                  VALUES (@Name, @Year, @IsActive, @CreatedAt, @UpdatedAt)",
                new { 
                    session.Name, 
                    session.Year, 
                    session.IsActive, 
                    CreatedAt = now,
                    UpdatedAt = now
                });
                
            return result > 0;
        }

        public async Task<bool> UpdateSessionAsync(CalendarSession session)
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            // Si cette session devient active, désactiver toutes les autres
            if (session.IsActive)
            {
                await connection.ExecuteAsync(
                    "UPDATE calendar_sessions SET is_active = 0, updated_at = @Now WHERE id != @Id AND is_active = 1",
                    new { Id = session.Id, Now = DateTime.UtcNow });
            }
            
            var result = await connection.ExecuteAsync(
                @"UPDATE calendar_sessions 
                  SET name = @Name, year = @Year, is_active = @IsActive, updated_at = @UpdatedAt 
                  WHERE id = @Id",
                new { 
                    session.Id,
                    session.Name, 
                    session.Year, 
                    session.IsActive,
                    UpdatedAt = DateTime.UtcNow
                });
                
            return result > 0;
        }

        public async Task<bool> SetSessionActiveAsync(int id)
        {
            try
            {
                using var connection = new MySqlConnection(_dbConfig.ConnectionString);
                await connection.OpenAsync();
                
                Console.WriteLine($"Désactivation de toutes les sessions actives...");
                // Désactiver toutes les sessions
                var deactivated = await connection.ExecuteAsync(
                    "UPDATE calendar_sessions SET is_active = 0, updated_at = @Now WHERE is_active = 1",
                    new { Now = DateTime.UtcNow });
                Console.WriteLine($"Sessions désactivées: {deactivated}");
                
                Console.WriteLine($"Activation de la session avec ID {id}...");
                // Activer la session demandée
                var result = await connection.ExecuteAsync(
                    "UPDATE calendar_sessions SET is_active = 1, updated_at = @Now WHERE id = @Id",
                    new { Id = id, Now = DateTime.UtcNow });
                Console.WriteLine($"Résultat de l'activation: {result}");
                
                // Vérifier que la session a bien été activée
                var checkActive = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM calendar_sessions WHERE id = @Id AND is_active = 1",
                    new { Id = id });
                Console.WriteLine($"Vérification de l'activation: {checkActive} (1 = succès, 0 = échec)");
                
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'activation de la session: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}