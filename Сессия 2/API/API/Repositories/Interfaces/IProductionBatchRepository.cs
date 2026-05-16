using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories.Interfaces;

public interface IProductionBatchRepository
{
    Task<IEnumerable<ProductionBatch>> GetAllAsync(DateTime? from = null, DateTime? to = null);
    Task<ProductionBatch?> GetByIdAsync(int id);
    Task<ProductionBatchDetailDto?> GetBatchDetailsAsync(int id);
    Task<IEnumerable<ProductionBatch>> GetActiveBatchesAsync();
    Task<int> StartBatchAsync(StartProductionBatchDto dto, int userId);
    Task<bool> CompleteBatchAsync(int batchId, decimal actualQuantity, int userId);
    Task<bool> CancelBatchAsync(int batchId, int userId);
    Task<bool> UpdateLabDecisionAsync(int batchId, string decision, string? reason, int userId);
}
