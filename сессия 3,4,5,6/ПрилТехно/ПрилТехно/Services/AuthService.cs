using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ПрилТехно.Models;

namespace ПрилТехно.Services
{
    public class AuthService : IAuthService
    {
        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);
        private readonly ApiClient _apiClient;
        private string? _token;
        private UserProfile? _currentUser;

        public AuthService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }


        public UserProfile? CurrentUser => _currentUser;
        public string? Token => _token;

        public async Task<bool> LoginAsync(string username, string password)
        {
            var request = new LoginRequest { Username = username, Password = password };
            var response = await _apiClient.PostAsync<object>("/api/auth/login", request);  // object, а не dynamic

            if (response?.IsSuccess != true || response.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"Login failed: {response?.ErrorMessage}");
                return false;
            }

            try
            {
                // response.Data - это JsonElement, полученный из API
                var data = System.Text.Json.JsonSerializer.Serialize(response.Data);
                using var doc = System.Text.Json.JsonDocument.Parse(data);
                var root = doc.RootElement;

                string token = root.GetProperty("token").GetString() ?? string.Empty;
                var userElement = root.GetProperty("user");

                var user = new UserProfile
                {
                    Id = userElement.GetProperty("id").GetInt32(),
                    Username = userElement.GetProperty("username").GetString() ?? string.Empty,
                    FullName = userElement.GetProperty("fullName").GetString() ?? string.Empty,
                    Email = userElement.GetProperty("email").GetString() ?? string.Empty,
                    Role = userElement.GetProperty("role").GetString() ?? string.Empty,
                    Department = userElement.GetProperty("department").GetString() ?? string.Empty,
                    PhotoBase64 = userElement.TryGetProperty("photoBase64", out var photo) ? photo.GetString() : null
                };

                if (!string.IsNullOrEmpty(token))
                {
                    _token = token;
                    _currentUser = user;
                    _apiClient.SetToken(token);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка разбора ответа: {ex.Message}");
            }
            return false;
        }

        public void Logout()
        {
            _token = null;
            _currentUser = null;
        }
    }
}
