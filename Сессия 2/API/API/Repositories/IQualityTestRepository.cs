using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories.Interfaces;

public interface IQualityTestRepository
{
    Task<IEnumerable<QualityTest>> GetTestsByBatchIdAsync(int batchId);
    Task<QualityTest?> GetTestWithResultsAsync(int testId);
    Task<int> CreateTestAsync(CreateQualityTestDto dto, int userId);
    Task<bool> EnterResultsAsync(EnterTestResultDto dto, int userId);
    Task<bool> CompleteTestAsync(int testId);
    Task<bool> AreAllTestsCompletedAsync(int batchId);
}
