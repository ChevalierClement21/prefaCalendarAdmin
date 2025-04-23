using Dapper;
using MySqlConnector;
using prefaCalendarAdmin.Config;
using prefaCalendarAdmin.Models.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace prefaCalendarAdmin.Services
{
    public class SessionStatsService : ISessionStatsService
    {
        private readonly DatabaseConfig _dbConfig;

        public SessionStatsService(DatabaseConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<SessionStats> GetSessionStatsAsync(int sessionId)
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();
            
            var stats = new SessionStats();
            
            // Récupérer tous les secteurs
            var sectors = await connection.QueryAsync<SectorData>(@"
                SELECT id, name, color FROM sectors
            ");
            
            // Pour chaque secteur, récupérer les statistiques
            foreach (var sector in sectors)
            {
                // Nombre de tournées pour ce secteur dans cette session
                var toursCount = await connection.ExecuteScalarAsync<int>(@"
                    SELECT COUNT(*) FROM tours
                    WHERE sector_id = @SectorId AND session_id = @SessionId
                ", new { SectorId = sector.Id, SessionId = sessionId });
                
                if (toursCount > 0)
                {
                    // Récupérer le montant total et le nombre de calendriers vendus pour ce secteur
                    var sectorStats = await connection.QueryFirstOrDefaultAsync<(decimal TotalAmount, int CalendarsSold)>(@"
                        SELECT 
                            COALESCE(SUM(tc.total_amount), 0) as TotalAmount,
                            COALESCE(SUM(tc.calendars_sold), 0) as CalendarsSold
                        FROM tours t
                        LEFT JOIN tour_completions tc ON t.id = tc.tour_id
                        WHERE t.sector_id = @SectorId AND t.session_id = @SessionId
                    ", new { SectorId = sector.Id, SessionId = sessionId });
                    
                    sector.ToursCount = toursCount;
                    sector.TotalAmount = sectorStats.TotalAmount;
                    sector.CalendarsSold = sectorStats.CalendarsSold;
                    
                    stats.Sectors.Add(sector);
                    
                    // Ajouter au total global
                    stats.TotalAmount += sectorStats.TotalAmount;
                    stats.TotalCalendarsSold += sectorStats.CalendarsSold;
                }
            }
            
            return stats;
        }
    }
}
