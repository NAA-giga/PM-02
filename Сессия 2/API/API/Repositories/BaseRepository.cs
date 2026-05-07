using Microsoft.Data.SqlClient;
using System.Data;
namespace API.Repositories
{
    public abstract class BaseRepository
    {
        private readonly IConfiguration _configuration;
        protected readonly string _connectionString;

        protected BaseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        protected IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
