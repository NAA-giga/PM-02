using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ПрилТехно.Services
{
    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string Token, int UserId, string Username, string Role);
    public record ApiResponse<T>(bool IsSuccess, T? Data, string? ErrorMessage, PaginationInfo? Pagination);
    public record PaginationInfo(int Page, int PageSize, int TotalCount);

    // Продукты
    public record ProductDto(int Id, string Code, string Name, string ProductType, string FormType, string Status);

    // Рецептуры
    public record RecipeDto(int Id, int ProductId, string ProductName, int Version, string Name, string Status, DateTime CreatedAt);
    public record RecipeComponentDto(int? Id, int RawMaterialId, string RawMaterialName, string RawMaterialCode,
                                     decimal Percentage, int LoadOrder, decimal? ToleranceMin, decimal? ToleranceMax);
    public record RecipeDetailsDto(int Id, int ProductId, string ProductName, int Version, string Name, string Status,
                                   DateTime? ApprovedAt, string? ApprovedByName, DateTime CreatedAt,
                                   List<RecipeComponentDto> Components);
    public record CreateRecipeDto(int ProductId, int Version, string Name, List<RecipeComponentDto> Components);
    public record UpdateRecipeDto(string Name, List<RecipeComponentDto> Components);

    // Технологические карты
    public record TechCardDto(int Id, string Name, int Version, string ProductName, string Status, DateTime CreatedAt);
    public record TechStepDto(int Id, int StepOrder, string StepName, string StepType, int? EquipmentId,
                              decimal? PlannedTempC, decimal? PlannedPressureBar, int? PlannedDurationMin,
                              bool IsMandatory, string? Instruction);
    public record TechCardDetailsDto(int Id, int ProductId, string ProductName, int Version, string Name, string? Description,
                                     string Status, DateTime? ApprovedAt, List<TechStepDto> Steps);
    public record CreateTechCardDto(int ProductId, int Version, string Name, string? Description, List<TechStepDto> Steps);

    // Производственные заказы
    public record ProductionOrderDto(int Id, string OrderNumber, string ProductName, decimal PlannedQuantityKg,
                                     string Status, DateTime PlannedStartDate);
    public record CreateProductionOrderDto(string OrderNumber, int ProductId, int RecipeId, int TechCardId,
                                           decimal PlannedQuantityKg, DateTime PlannedStartDate);

    // Производственные партии
    public record ProductionBatchDto(int Id, string BatchNumber, string ProductName, string Status,
                                     DateTime? StartTime, DateTime? EndTime,
                                     decimal PlannedQuantityKg, decimal? ActualQuantityKg);
    public record BatchStepDto(int StepOrder, string StepName, string Status, decimal? ActualTempC,
                               decimal? ActualPressureBar, int? ActualDurationMin, bool DeviationFlag, string? OperatorComment);
    public record ProductionBatchDetailsDto(int Id, string BatchNumber, string OrderNumber, string ProductName,
                                            string Status, DateTime? StartTime, DateTime? EndTime,
                                            List<BatchStepDto> Steps);
    public record StartBatchDto(int OrderId, string BatchNumber, decimal PlannedQuantityKg);

    // Отклонения
    public record DeviationDto(int Id, string BatchNumber, string DeviationType, string Severity, string Description,
                               string? ParameterName, string? PlannedValue, string? ActualValue, DateTime CreatedAt);

    // ----------------------------------------------------------------------
    // Дженерик-контекст для Source Generation (System.Text.Json)
    // ----------------------------------------------------------------------
    [JsonSerializable(typeof(LoginRequest))]
    [JsonSerializable(typeof(LoginResponse))]
    [JsonSerializable(typeof(ApiResponse<ProductDto[]>))]
    [JsonSerializable(typeof(ProductDto[]))]
    [JsonSerializable(typeof(ApiResponse<RecipeDto[]>))]
    [JsonSerializable(typeof(RecipeDto[]))]
    [JsonSerializable(typeof(RecipeDetailsDto))]
    [JsonSerializable(typeof(ApiResponse<RecipeDetailsDto>))]
    [JsonSerializable(typeof(CreateRecipeDto))]
    [JsonSerializable(typeof(UpdateRecipeDto))]
    [JsonSerializable(typeof(TechCardDto[]))]
    [JsonSerializable(typeof(TechCardDetailsDto))]
    [JsonSerializable(typeof(CreateTechCardDto))]
    [JsonSerializable(typeof(ProductionOrderDto[]))]
    [JsonSerializable(typeof(CreateProductionOrderDto))]
    [JsonSerializable(typeof(ProductionBatchDto[]))]
    [JsonSerializable(typeof(ProductionBatchDetailsDto))]
    [JsonSerializable(typeof(StartBatchDto))]
    [JsonSerializable(typeof(DeviationDto[]))]
    [JsonSerializable(typeof(ApiResponse<int>))]
    [JsonSerializable(typeof(ApiResponse<object>))]
    public partial class AppJsonContext : JsonSerializerContext;

    // ----------------------------------------------------------------------
    // Основной сервис API с внедрением HttpClient
    // ----------------------------------------------------------------------
    public class ApiService(HttpClient _httpClient)
    {
        private readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = AppJsonContext.Default
        };

        /// <summary>
        /// Установка токена авторизации для всех последующих запросов
        /// </summary>
        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Базовый метод для выполнения HTTP-запросов с десериализацией ответа
        /// </summary>
        private async Task<T> SendAsync<T>(HttpMethod method, string url, object? body = null)
        {
            var request = new HttpRequestMessage(method, url);
            if (body is not null)
            {
                var json = JsonSerializer.Serialize(body, _jsonOpts);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"API error ({response.StatusCode}): {content}");

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOpts);
            if (apiResponse is null || !apiResponse.IsSuccess)
                throw new InvalidOperationException(apiResponse?.ErrorMessage ?? "Unknown API error");

            return apiResponse.Data!;
        }

        // ------------------------- Авторизация -------------------------
        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            var result = await SendAsync<LoginResponse>(HttpMethod.Post, "Auth/login", new LoginRequest(username, password));
            SetToken(result.Token);
            return result;
        }

        // ------------------------- Продукты -------------------------
        public async Task<ProductDto[]> GetProductsAsync()
            => await SendAsync<ProductDto[]>(HttpMethod.Get, "Reference/products");

        // ------------------------- Рецептуры -------------------------
        public async Task<RecipeDto[]> GetRecipesAsync(int? productId = null, string? status = null)
        {
            var url = "Recipes";
            var query = new List<string>();
            if (productId.HasValue) query.Add($"productId={productId}");
            if (!string.IsNullOrEmpty(status)) query.Add($"status={status}");
            if (query.Count > 0) url += "?" + string.Join("&", query);
            return await SendAsync<RecipeDto[]>(HttpMethod.Get, url);
        }

        public async Task<RecipeDetailsDto> GetRecipeDetailsAsync(int id)
            => await SendAsync<RecipeDetailsDto>(HttpMethod.Get, $"Recipes/{id}");

        public async Task<int> CreateRecipeAsync(CreateRecipeDto dto)
            => await SendAsync<int>(HttpMethod.Post, "Recipes", dto);

        public async Task UpdateRecipeAsync(int id, UpdateRecipeDto dto)
            => await SendAsync<object>(HttpMethod.Put, $"Recipes/{id}", dto);

        public async Task ApproveRecipeAsync(int id)
            => await SendAsync<object>(HttpMethod.Post, $"Recipes/{id}/approve");

        public async Task ArchiveRecipeAsync(int id)
            => await SendAsync<object>(HttpMethod.Post, $"Recipes/{id}/archive");

        // ------------------------- Технологические карты -------------------------
        public async Task<TechCardDto[]> GetTechCardsAsync()
            => await SendAsync<TechCardDto[]>(HttpMethod.Get, "TechCards");

        public async Task<TechCardDetailsDto> GetTechCardDetailsAsync(int id)
            => await SendAsync<TechCardDetailsDto>(HttpMethod.Get, $"TechCards/{id}");

        public async Task<int> CreateTechCardAsync(CreateTechCardDto dto)
            => await SendAsync<int>(HttpMethod.Post, "TechCards", dto);

        public async Task ApproveTechCardAsync(int id)
            => await SendAsync<object>(HttpMethod.Post, $"TechCards/{id}/approve");

        // ------------------------- Заказы -------------------------
        public async Task<ProductionOrderDto[]> GetProductionOrdersAsync()
            => await SendAsync<ProductionOrderDto[]>(HttpMethod.Get, "ProductionOrders");

        public async Task<int> CreateProductionOrderAsync(CreateProductionOrderDto dto)
            => await SendAsync<int>(HttpMethod.Post, "ProductionOrders", dto);

        public async Task ConfirmOrderAsync(int id)
            => await SendAsync<object>(HttpMethod.Post, $"ProductionOrders/{id}/confirm");

        // ------------------------- Партии -------------------------
        public async Task<ProductionBatchDto[]> GetProductionBatchesAsync()
            => await SendAsync<ProductionBatchDto[]>(HttpMethod.Get, "ProductionBatches");

        public async Task<ProductionBatchDetailsDto> GetBatchDetailsAsync(int id)
            => await SendAsync<ProductionBatchDetailsDto>(HttpMethod.Get, $"ProductionBatches/{id}");

        public async Task<int> StartBatchAsync(StartBatchDto dto)
            => await SendAsync<int>(HttpMethod.Post, "ProductionBatches/start", dto);

        // ------------------------- Отклонения -------------------------
        public async Task<DeviationDto[]> GetDeviationsForBatchAsync(int batchId)
            => await SendAsync<DeviationDto[]>(HttpMethod.Get, $"Deviations/batch/{batchId}");

        // ------------------------- Утилиты -------------------------
        /// <summary>
        /// Сброс токена (при выходе из системы)
        /// </summary>
        public void ClearToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}