using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories.Interfaces;

public interface IBatchStepExecutionRepository
{
    Task<bool> StartStepAsync(int batchId, int stepOrder, int userId);
    Task<bool> CompleteStepAsync(int batchId, int stepOrder, PerformStepDto data, int userId);
}
