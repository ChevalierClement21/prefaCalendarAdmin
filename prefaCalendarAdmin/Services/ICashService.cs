using prefaCalendarAdmin.Models.Cash;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace prefaCalendarAdmin.Services
{
    public interface ICashService
    {
        Task<DetailedCashView> GetSessionCashSummaryAsync(int sessionId);
        Task<List<TourCompletion>> GetAllCashDetailsAsync();
        Task<TourCompletion?> GetTourCashDetailAsync(int tourId);
        Task<List<DetailedCashView>> GetAllSessionsCashSummaryAsync();
    }
}
