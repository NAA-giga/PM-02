using Microsoft.Data.SqlClient;
using System.Data;

namespace API.Repositories;

public abstract class BaseRepository
{
    protected readonly string _connectionString;

    protected BaseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found.");
    }

    protected IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
