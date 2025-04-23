using prefaCalendarAdmin.Models.Session;
using System.Threading.Tasks;

namespace prefaCalendarAdmin.Services
{
    public interface ISessionStatsService
    {
        Task<SessionStats> GetSessionStatsAsync(int sessionId);
    }
}
