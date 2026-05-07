using API.Models.Entities;

namespace API.Repositories;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
}