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
    public async Task<int> CreateAsync(Product product)
    {
        using var conn = CreateConnection();
        const string sql = @"
        INSERT INTO products (code, name, product_type, form_type, status, created_at, updated_at)
        VALUES (@Code, @Name, @ProductType, @FormType, @Status, @CreatedAt, @UpdatedAt);
        SELECT CAST(SCOPE_IDENTITY() AS INT)";
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        return await conn.ExecuteScalarAsync<int>(sql, product);
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        using var conn = CreateConnection();
        const string sql = @"
        UPDATE products 
        SET code = @Code,
            name = @Name,
            product_type = @ProductType,
            form_type = @FormType,
            status = @Status,
            updated_at = @UpdatedAt
        WHERE id = @Id";
        product.UpdatedAt = DateTime.UtcNow;
        var rows = await conn.ExecuteAsync(sql, product);
        return rows > 0;
    }

    public async Task<bool> ArchiveAsync(int id)
    {
        using var conn = CreateConnection();
        const string sql = "UPDATE products SET status = 'archived', updated_at = GETDATE() WHERE id = @Id";
        var rows = await conn.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}