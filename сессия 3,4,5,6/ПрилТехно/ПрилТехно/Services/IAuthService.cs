using System;
using System.Collections.Generic;
using System.Text;
using ПрилТехно.Models;

namespace ПрилТехно.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        void Logout();
        bool IsAuthenticated { get; }
        UserProfile? CurrentUser { get; }
        string? Token { get; }
    }
}
