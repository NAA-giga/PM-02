using Dapper;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories;

public class DepartmentRepository : BaseRepository, IDepartmentRepository
{
    public DepartmentRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM Departments ORDER BY Name";
        return await connection.QueryAsync<Department>(sql);
    }
}