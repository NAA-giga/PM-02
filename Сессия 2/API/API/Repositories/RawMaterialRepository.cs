// Repositories/RawMaterialRepository.cs

using Dapper;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories;

public class RawMaterialRepository : BaseRepository, IRawMaterialRepository
{
    public RawMaterialRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<RawMaterial>> GetAllAsync()
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM raw_materials WHERE is_active = 1 ORDER BY name";
        return await conn.QueryAsync<RawMaterial>(sql);
    }

    public async Task<RawMaterial?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM raw_materials WHERE id = @Id";
        return await conn.QueryFirstOrDefaultAsync<RawMaterial>(sql, new { Id = id });
    }
}