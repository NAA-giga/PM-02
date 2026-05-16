using API.Models.Entities;
namespace API.Repositories.Interfaces
{
    public interface IExtruderTelemetryRepository
    {
        Task<IEnumerable<ExtruderTelemetry>> GetByBatchIdAsync(int batchId);
        Task<bool> AddTelemetryAsync(ExtruderTelemetry telemetry);
        Task<bool> AddBatchAsync(IEnumerable<ExtruderTelemetry> telemetryList);
    }
}
