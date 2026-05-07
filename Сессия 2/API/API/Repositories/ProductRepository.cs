using Dapper;
using API.Models.Entities;

namespace API.Repositories;

public class ProductRepository : BaseRepository, IProductRepository
{
    public ProductRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM Products WHERE status = 'active' ORDER BY Name";
        return await connection.QueryAsync<Product>(sql);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM Products WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
    }
}