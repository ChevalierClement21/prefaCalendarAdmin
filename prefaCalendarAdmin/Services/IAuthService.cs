using prefaCalendarAdmin.Models;
using System.Threading.Tasks;

namespace prefaCalendarAdmin.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginModel loginModel);
        Task Logout();
        Task<bool> IsAuthenticatedAsync();
        Task<bool> IsAdminAsync();
        Task<UserWithRoles?> GetCurrentUserAsync();
    }
}
