using System.Threading.Tasks;
using ПрогЛабор.Models;

namespace ПрогЛабор.Services
{
    public class AuthService : IAuthService
    {
        private UserProfile? _currentUser;

        public UserProfile? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public async Task<bool> LoginAsync(string username, string password)
        {
            // Для теста захардкодим лаборанта
            if (username == "lab" && password == "lab")
            {
                _currentUser = new UserProfile
                {
                    Id = 5, // предположим ID лаборанта в БД = 5
                    Username = "lab",
                    FullName = "Лаборант Иванов",
                    Role = "lab_analyst"
                };
                return true;
            }
            return false;
        }

        public void Logout()
        {
            _currentUser = null;
        }
    }
}