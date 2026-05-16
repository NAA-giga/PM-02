using API.Models.DTOs;

namespace API.Repositories.Interfaces
{
    public interface IDeviationRepository
    {
        Task<IEnumerable<DeviationDto>> GetAllAsync(DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<DeviationDto>> GetByBatchIdAsync(int batchId);
        Task<int> CreateAsync(ReportDeviationDto dto, int userId);
    }
}
