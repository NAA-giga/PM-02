using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories.Interfaces;

public interface IProductionBatchRepository
{
    Task<IEnumerable<ProductionBatch>> GetActiveBatchesAsync();
    Task<ProductionBatch?> GetByIdAsync(int id);
    Task<ProductionBatchResponseDto?> GetBatchWithStepsAsync(int id);
    Task<int> StartBatchAsync(StartProductionBatchDto dto, int userId);
    Task<bool> CompleteBatchAsync(int batchId, decimal actualQuantity, int userId);
    Task<bool> UpdateBatchStatusAsync(int batchId, string status);
    Task<bool> UpdateLabDecisionAsync(int batchId, string decision, string? reason, int userId);
}
