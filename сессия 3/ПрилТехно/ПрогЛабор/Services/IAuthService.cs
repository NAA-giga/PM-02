using ПрогЛабор.Models;

namespace ПрогЛабор.Services
{
    public interface IAuthService
    {
        UserProfile? CurrentUser { get; }
        Task<bool> LoginAsync(string username, string password);
        void Logout();
        bool IsAuthenticated { get; }
    }
}