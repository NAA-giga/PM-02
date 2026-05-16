using API.Models.Entities;

namespace API.Repositories.Interfaces;

public interface IRawMaterialRepository
{
    Task<IEnumerable<RawMaterial>> GetAllAsync();
    Task<RawMaterial?> GetByIdAsync(int id);
}   