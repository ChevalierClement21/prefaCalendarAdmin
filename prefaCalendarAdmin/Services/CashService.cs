using Dapper;
using MySqlConnector;
using prefaCalendarAdmin.Config;
using prefaCalendarAdmin.Models.Cash;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace prefaCalendarAdmin.Services
{
    public class CashService : ICashService
    {
        private readonly DatabaseConfig _dbConfig;

        public CashService(DatabaseConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<DetailedCashView> GetSessionCashSummaryAsync(int sessionId)
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();

            // Récupérer les informations de la session
            var sessionResult = await connection.QueryFirstOrDefaultAsync(@"
                SELECT id, name FROM calendar_sessions WHERE id = @SessionId
            ", new { SessionId = sessionId });

            if (sessionResult == null)
                return new DetailedCashView();

            var summary = new DetailedCashView
            {
                SessionId = Convert.ToInt32(sessionResult.id),
                SessionName = sessionResult.name ?? string.Empty
            };

            // Récupérer tous les détails des tournées pour cette session
            var tours = await GetToursBySessionAsync(connection, sessionId);
            summary.Tours = tours;

            return summary;
        }

        public async Task<List<TourCompletion>> GetAllCashDetailsAsync()
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();

            return await GetAllToursAsync(connection);
        }

        public async Task<TourCompletion?> GetTourCashDetailAsync(int tourId)
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    t.id as TourId,
                    t.name as TourName,
                    s.name as SectorName,
                    cs.id as SessionId,
                    cs.name as SessionName,
                    COALESCE(tc.calendars_sold, 0) as CalendarsSold,
                    COALESCE(tc.tickets_5, 0) as Tickets5,
                    COALESCE(tc.tickets_10, 0) as Tickets10,
                    COALESCE(tc.tickets_20, 0) as Tickets20,
                    COALESCE(tc.tickets_50, 0) as Tickets50,
                    COALESCE(tc.tickets_100, 0) as Tickets100,
                    COALESCE(tc.tickets_200, 0) as Tickets200,
                    COALESCE(tc.tickets_500, 0) as Tickets500,
                    COALESCE(tc.coins_1c, 0) as Coins1c,
                    COALESCE(tc.coins_2c, 0) as Coins2c,
                    COALESCE(tc.coins_5c, 0) as Coins5c,
                    COALESCE(tc.coins_10c, 0) as Coins10c,
                    COALESCE(tc.coins_20c, 0) as Coins20c,
                    COALESCE(tc.coins_50c, 0) as Coins50c,
                    COALESCE(tc.coins_1e, 0) as Coins1e,
                    COALESCE(tc.coins_2e, 0) as Coins2e,
                    COALESCE(tc.check_count, 0) as CheckCount,
                    COALESCE(tc.check_total_amount, 0) as CheckTotalAmount,
                    COALESCE(tc.check_amounts, '[]') as CheckAmountsJson,
                    COALESCE(tc.total_amount, 0) as TotalAmount,
                    COALESCE(tc.notes, '') as Notes
                FROM tours t
                LEFT JOIN sectors s ON t.sector_id = s.id
                LEFT JOIN calendar_sessions cs ON t.session_id = cs.id
                LEFT JOIN tour_completions tc ON t.id = tc.tour_id
                WHERE t.id = @TourId
            ";

            var result = await connection.QueryFirstOrDefaultAsync(query, new { TourId = tourId });
            if (result == null) return null;

            return MapToTourCompletion(result);
        }

        public async Task<List<DetailedCashView>> GetAllSessionsCashSummaryAsync()
        {
            using var connection = new MySqlConnection(_dbConfig.ConnectionString);
            await connection.OpenAsync();

            // Récupérer toutes les sessions
            var sessionResults = await connection.QueryAsync(@"
                SELECT id, name FROM calendar_sessions ORDER BY year DESC, name
            ");

            var summaries = new List<DetailedCashView>();

            foreach (var sessionResult in sessionResults)
            {
                var summary = new DetailedCashView
                {
                    SessionId = Convert.ToInt32(sessionResult.id),
                    SessionName = sessionResult.name ?? string.Empty,
                    Tours = await GetToursBySessionAsync(connection, Convert.ToInt32(sessionResult.id))
                };

                summaries.Add(summary);
            }

            // Ajouter les tournées sans session assignée
            var toursWithoutSession = await GetToursWithoutSessionAsync(connection);
            if (toursWithoutSession.Any())
            {
                summaries.Add(new DetailedCashView
                {
                    SessionId = -1,
                    SessionName = "Tournées sans session",
                    Tours = toursWithoutSession
                });
            }

            return summaries;
        }

        private async Task<List<TourCompletion>> GetToursBySessionAsync(MySqlConnection connection, int sessionId)
        {
            var query = @"
                SELECT 
                    t.id as TourId,
                    t.name as TourName,
                    s.name as SectorName,
                    cs.id as SessionId,
                    cs.name as SessionName,
                    COALESCE(tc.calendars_sold, 0) as CalendarsSold,
                    COALESCE(tc.tickets_5, 0) as Tickets5,
                    COALESCE(tc.tickets_10, 0) as Tickets10,
                    COALESCE(tc.tickets_20, 0) as Tickets20,
                    COALESCE(tc.tickets_50, 0) as Tickets50,
                    COALESCE(tc.tickets_100, 0) as Tickets100,
                    COALESCE(tc.tickets_200, 0) as Tickets200,
                    COALESCE(tc.tickets_500, 0) as Tickets500,
                    COALESCE(tc.coins_1c, 0) as Coins1c,
                    COALESCE(tc.coins_2c, 0) as Coins2c,
                    COALESCE(tc.coins_5c, 0) as Coins5c,
                    COALESCE(tc.coins_10c, 0) as Coins10c,
                    COALESCE(tc.coins_20c, 0) as Coins20c,
                    COALESCE(tc.coins_50c, 0) as Coins50c,
                    COALESCE(tc.coins_1e, 0) as Coins1e,
                    COALESCE(tc.coins_2e, 0) as Coins2e,
                    COALESCE(tc.check_count, 0) as CheckCount,
                    COALESCE(tc.check_total_amount, 0) as CheckTotalAmount,
                    COALESCE(tc.check_amounts, '[]') as CheckAmountsJson,
                    COALESCE(tc.total_amount, 0) as TotalAmount,
                    COALESCE(tc.notes, '') as Notes
                FROM tours t
                LEFT JOIN sectors s ON t.sector_id = s.id
                LEFT JOIN calendar_sessions cs ON t.session_id = cs.id
                LEFT JOIN tour_completions tc ON t.id = tc.tour_id
                WHERE t.session_id = @SessionId
                ORDER BY s.name, t.name
            ";

            var results = await connection.QueryAsync(query, new { SessionId = sessionId });
            return results.Select(MapToTourCompletion).ToList();
        }

        private async Task<List<TourCompletion>> GetToursWithoutSessionAsync(MySqlConnection connection)
        {
            var query = @"
                SELECT 
                    t.id as TourId,
                    t.name as TourName,
                    s.name as SectorName,
                    0 as SessionId,
                    'Aucune session' as SessionName,
                    COALESCE(tc.calendars_sold, 0) as CalendarsSold,
                    COALESCE(tc.tickets_5, 0) as Tickets5,
                    COALESCE(tc.tickets_10, 0) as Tickets10,
                    COALESCE(tc.tickets_20, 0) as Tickets20,
                    COALESCE(tc.tickets_50, 0) as Tickets50,
                    COALESCE(tc.tickets_100, 0) as Tickets100,
                    COALESCE(tc.tickets_200, 0) as Tickets200,
                    COALESCE(tc.tickets_500, 0) as Tickets500,
                    COALESCE(tc.coins_1c, 0) as Coins1c,
                    COALESCE(tc.coins_2c, 0) as Coins2c,
                    COALESCE(tc.coins_5c, 0) as Coins5c,
                    COALESCE(tc.coins_10c, 0) as Coins10c,
                    COALESCE(tc.coins_20c, 0) as Coins20c,
                    COALESCE(tc.coins_50c, 0) as Coins50c,
                    COALESCE(tc.coins_1e, 0) as Coins1e,
                    COALESCE(tc.coins_2e, 0) as Coins2e,
                    COALESCE(tc.check_count, 0) as CheckCount,
                    COALESCE(tc.check_total_amount, 0) as CheckTotalAmount,
                    COALESCE(tc.check_amounts, '[]') as CheckAmountsJson,
                    COALESCE(tc.total_amount, 0) as TotalAmount,
                    COALESCE(tc.notes, '') as Notes
                FROM tours t
                LEFT JOIN sectors s ON t.sector_id = s.id
                LEFT JOIN tour_completions tc ON t.id = tc.tour_id
                WHERE t.session_id IS NULL
                ORDER BY s.name, t.name
            ";

            var results = await connection.QueryAsync(query);
            return results.Select(MapToTourCompletion).ToList();
        }

        private async Task<List<TourCompletion>> GetAllToursAsync(MySqlConnection connection)
        {
            var query = @"
                SELECT 
                    t.id as TourId,
                    t.name as TourName,
                    s.name as SectorName,
                    cs.id as SessionId,
                    cs.name as SessionName,
                    COALESCE(tc.calendars_sold, 0) as CalendarsSold,
                    COALESCE(tc.tickets_5, 0) as Tickets5,
                    COALESCE(tc.tickets_10, 0) as Tickets10,
                    COALESCE(tc.tickets_20, 0) as Tickets20,
                    COALESCE(tc.tickets_50, 0) as Tickets50,
                    COALESCE(tc.tickets_100, 0) as Tickets100,
                    COALESCE(tc.tickets_200, 0) as Tickets200,
                    COALESCE(tc.tickets_500, 0) as Tickets500,
                    COALESCE(tc.coins_1c, 0) as Coins1c,
                    COALESCE(tc.coins_2c, 0) as Coins2c,
                    COALESCE(tc.coins_5c, 0) as Coins5c,
                    COALESCE(tc.coins_10c, 0) as Coins10c,
                    COALESCE(tc.coins_20c, 0) as Coins20c,
                    COALESCE(tc.coins_50c, 0) as Coins50c,
                    COALESCE(tc.coins_1e, 0) as Coins1e,
                    COALESCE(tc.coins_2e, 0) as Coins2e,
                    COALESCE(tc.check_count, 0) as CheckCount,
                    COALESCE(tc.check_total_amount, 0) as CheckTotalAmount,
                    COALESCE(tc.check_amounts, '[]') as CheckAmountsJson,
                    COALESCE(tc.total_amount, 0) as TotalAmount,
                    COALESCE(tc.notes, '') as Notes
                FROM tours t
                LEFT JOIN sectors s ON t.sector_id = s.id
                LEFT JOIN calendar_sessions cs ON t.session_id = cs.id
                LEFT JOIN tour_completions tc ON t.id = tc.tour_id
                ORDER BY cs.name, s.name, t.name
            ";

            var results = await connection.QueryAsync(query);
            return results.Select(MapToTourCompletion).ToList();
        }

        private static TourCompletion MapToTourCompletion(dynamic result)
        {
            var checkAmounts = new List<decimal>();
            try
            {
                if (!string.IsNullOrEmpty(result.CheckAmountsJson))
                {
                    checkAmounts = JsonSerializer.Deserialize<List<decimal>>(result.CheckAmountsJson) ?? new List<decimal>();
                }
            }
            catch
            {
                // En cas d'erreur de désérialisation, garder la liste vide
            }

            var detail = new TourCompletion
            {
                TourId = Convert.ToInt32(result.TourId),
                TourName = result.TourName ?? string.Empty,
                SectorName = result.SectorName ?? string.Empty,
                SessionId = Convert.ToInt32(result.SessionId),
                SessionName = result.SessionName ?? string.Empty,
                CalendarsSold = Convert.ToInt32(result.CalendarsSold),
                Banknotes = new BanknotesDetail
                {
                    Tickets5 = Convert.ToInt32(result.Tickets5),
                    Tickets10 = Convert.ToInt32(result.Tickets10),
                    Tickets20 = Convert.ToInt32(result.Tickets20),
                    Tickets50 = Convert.ToInt32(result.Tickets50),
                    Tickets100 = Convert.ToInt32(result.Tickets100),
                    Tickets200 = Convert.ToInt32(result.Tickets200),
                    Tickets500 = Convert.ToInt32(result.Tickets500)
                },
                Coins = new CoinsDetail
                {
                    Coins1c = Convert.ToInt32(result.Coins1c),
                    Coins2c = Convert.ToInt32(result.Coins2c),
                    Coins5c = Convert.ToInt32(result.Coins5c),
                    Coins10c = Convert.ToInt32(result.Coins10c),
                    Coins20c = Convert.ToInt32(result.Coins20c),
                    Coins50c = Convert.ToInt32(result.Coins50c),
                    Coins1e = Convert.ToInt32(result.Coins1e),
                    Coins2e = Convert.ToInt32(result.Coins2e)
                },
                Checks = new ChecksDetail
                {
                    CheckCount = Convert.ToInt32(result.CheckCount),
                    CheckTotalAmount = Convert.ToDecimal(result.CheckTotalAmount),
                    CheckAmounts = checkAmounts
                },
                TotalAmount = Convert.ToDecimal(result.TotalAmount),
                Notes = result.Notes ?? string.Empty
            };

            // Calculer les totaux
            detail.BanknotesTotal = detail.Banknotes.GetTotal();
            detail.CoinsTotal = detail.Coins.GetTotal();
            detail.ChecksTotal = detail.Checks.CheckTotalAmount;

            return detail;
        }
    }
}
