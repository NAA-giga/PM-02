using System;
using System.Collections.Generic;
using System.Text;
using прогОпер.Models;

namespace прогОпер.Services
{
    public interface IAuthService
    {
        UserProfile? CurrentUser { get; }
        Task<bool> LoginAsync(string username, string password);
        void Logout();
        bool IsAuthenticated { get; }
    }
}
