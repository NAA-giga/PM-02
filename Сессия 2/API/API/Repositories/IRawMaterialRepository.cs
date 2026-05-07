using API.Models.Entities;

namespace API.Repositories;

public interface IRawMaterialRepository
{
    Task<IEnumerable<RawMaterial>> GetAllAsync();
    Task<RawMaterial?> GetByIdAsync(int id);
}   