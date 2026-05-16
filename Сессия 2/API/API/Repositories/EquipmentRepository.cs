using Dapper;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories;

public class EquipmentRepository : BaseRepository, IEquipmentRepository
{
    public EquipmentRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<Equipment>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM Equipment WHERE IsActive = 1 ORDER BY Name";
        return await connection.QueryAsync<Equipment>(sql);
    }

    public async Task<Equipment?> GetByIdAsync(int id)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM Equipment WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Equipment>(sql, new { Id = id });
    }
}