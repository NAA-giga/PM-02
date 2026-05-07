using Dapper;
using API.Models.Entities;
namespace API.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            using var connection = CreateConnection();
            const string sql = "SELECT * FROM Users WHERE Username = @username";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<int> CreateUserAsync(User user)
        {   
            using var connection = CreateConnection();
            const string sql = @"
            INSERT INTO Users (Username, PasswordHash, FullName, Email, RoleId, DepartmentId, IsActive, CreatedAt)
            VALUES (@Username, @PasswordHash, @FullName, @Email, @RoleId, @DepartmentId, 1, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT)";
            user.CreatedAt = DateTime.UtcNow;
            return await connection.QuerySingleAsync<int>(sql, user);
        }

    }
}
