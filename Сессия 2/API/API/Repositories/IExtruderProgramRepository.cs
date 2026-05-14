using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories
{
    public interface IExtruderProgramRepository
    {
        Task<IEnumerable<ExtruderProgram>> GetAllAsync();
        Task<ExtruderProgram?> GetByIdAsync(int id);
        Task<int> CreateAsync(ExtruderProgramDto dto, int userId);
        Task<bool> UpdateAsync(int id, ExtruderProgramDto dto, int userId);
        Task<bool> DeleteAsync(int id);
    }
}
