using prefaCalendarAdmin.Models.Session;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace prefaCalendarAdmin.Services
{
    public interface ISessionService
    {
        Task<List<CalendarSession>> GetAllSessionsAsync();
        Task<CalendarSession> GetSessionByIdAsync(int id);
        Task<CalendarSession> GetActiveSessionAsync();
        Task<bool> CreateSessionAsync(CalendarSession session);
        Task<bool> UpdateSessionAsync(CalendarSession session);
        Task<bool> SetSessionActiveAsync(int id);
    }
}