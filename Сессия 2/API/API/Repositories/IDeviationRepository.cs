using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories.Interfaces;

public interface IDeviationRepository
{
    Task<int> CreateAsync(ReportDeviationDto dto, int userId);
    Task<IEnumerable<Deviation>> GetByBatchIdAsync(int batchId);
}
