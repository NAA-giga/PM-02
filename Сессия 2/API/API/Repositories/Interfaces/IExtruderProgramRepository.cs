using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories.Interfaces
{
    public interface IExtruderProgramRepository
    {
        Task<IEnumerable<ExtruderProgram>> GetAllAsync();
        Task<ExtruderProgram?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateExtruderProgramDto dto, int userId);
        Task<bool> UpdateAsync(int id, CreateExtruderProgramDto dto, int userId);
        Task<bool> DeleteAsync(int id);
        Task<bool> AssignToBatchAsync(int programId, int batchId);
    }
}
