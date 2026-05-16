using Dapper;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories;

public class RoleRepository : BaseRepository, IRoleRepository
{
    public RoleRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM Roles ORDER BY Name";
        return await connection.QueryAsync<Role>(sql);
    }
}