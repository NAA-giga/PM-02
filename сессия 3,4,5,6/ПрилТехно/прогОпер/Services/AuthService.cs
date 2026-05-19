using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using прогОпер.Models;

namespace прогОпер.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private UserProfile? _currentUser;

        public AuthService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public UserProfile? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public async Task<bool> LoginAsync(string username, string password)
        {
            using var conn = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT u.id, u.username, u.full_name, u.email, r.name AS Role, d.name AS Department
                FROM users u
                JOIN roles r ON u.role_id = r.id
                JOIN departments d ON u.department_id = d.id
                WHERE u.username = @Username AND u.is_active = 1";

            var user = await conn.QueryFirstOrDefaultAsync<UserProfile>(sql, new { Username = username });
            if (user == null) return false;

            // В реальном проекте пароль проверяется через BCrypt, здесь для демо прямое сравнение
            // Для теста используем пароль "operator" для роли operator
            if (password != "operator") return false;

            if (user.Role != "operator")
                return false;

            _currentUser = user;
            return true;
        }

        public void Logout()
        {
            _currentUser = null;
        }
    }
}
