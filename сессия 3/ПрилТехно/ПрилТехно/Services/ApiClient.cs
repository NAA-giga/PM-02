using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ПрилТехно.Models;

namespace ПрилТехно.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
        private string? _token;

        // Убрали IAuthService из конструктора
        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Вызывается из AuthService после успешного входа
        public void SetToken(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private void SetAuthorizationHeader()
        {
            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            }
        }

        public async Task<ApiResponse<T>?> GetAsync<T>(string uri)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync(uri);
            return await DeserializeResponse<T>(response);
        }

        public async Task<ApiResponse<T>?> PostAsync<T>(string uri, object data)
        {
            SetAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(uri, content);
            return await DeserializeResponse<T>(response);
        }

        public async Task<ApiResponse<T>?> PutAsync<T>(string uri, object data)
        {
            SetAuthorizationHeader();
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(uri, content);
            return await DeserializeResponse<T>(response);
        }

        public async Task<ApiResponse<T>?> DeleteAsync<T>(string uri)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync(uri);
            return await DeserializeResponse<T>(response);
        }

        private async Task<ApiResponse<T>?> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);
                return errorResponse ?? new ApiResponse<T> { IsSuccess = false, ErrorMessage = $"Ошибка HTTP {response.StatusCode}" };
            }
            return JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);
        }
    }
}