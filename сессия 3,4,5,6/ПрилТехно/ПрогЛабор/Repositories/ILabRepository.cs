using ПрогЛабор.Models;

public interface ILabRepository
{
    // ========== Сырьё ==========
    Task<List<RawMaterialBatchDto>> GetRawMaterialBatchesAsync(string? status = null);
    Task<RawMaterialBatchDto?> GetRawMaterialBatchByIdAsync(int id);
    Task<int> CreateRawMaterialTestAsync(int batchId, string testType, int userId);
    Task<RawMaterialTestDto?> GetRawMaterialTestByIdAsync(int testId);
    Task<List<RawMaterialTestResultDto>> GetRawMaterialTestResultsAsync(int testId);
    Task<bool> SaveRawMaterialTestResultAsync(RawMaterialTestResultDto result);
    Task<bool> CompleteRawMaterialTestAsync(int testId, int userId);
    Task<bool> SetRawMaterialBatchDecisionAsync(int batchId, string decision, string? reason, int userId);
    Task<bool> IsRawMaterialTestCompleted(int batchId); // проверка наличия завершённого испытания

    // ========== Готовая продукция ==========
    Task<List<ProductBatchForLabDto>> GetProductBatchesForLabAsync();
    Task<int> CreateQualityTestAsync(int batchId, string testType, int userId);
    Task<QualityTestDto?> GetQualityTestByIdAsync(int testId);
    Task<List<QualityTestResultDto>> GetQualityTestResultsAsync(int testId);
    Task<bool> SaveQualityTestResultAsync(QualityTestResultDto result);
    Task<bool> CompleteQualityTestAsync(int testId, int userId);
    Task<bool> SetProductBatchDecisionAsync(int batchId, string decision, string? reason, int userId);
    Task<bool> IsProductTestCompleted(int batchId);
}