using Dapper;
using API.Models.Entities;

namespace API.Repositories;

public class RawMaterialRepository : BaseRepository, IRawMaterialRepository
{
    public RawMaterialRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<RawMaterial>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM RawMaterials WHERE IsActive = 1 ORDER BY Name";
        return await connection.QueryAsync<RawMaterial>(sql);
    }

    public async Task<RawMaterial?> GetByIdAsync(int id)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM RawMaterials WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<RawMaterial>(sql, new { Id = id });
    }
}