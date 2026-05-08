using Dapper;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var connection = CreateConnection();
        const string sql = "SELECT * FROM Users WHERE Username = @Username";
        return await connection.QueryFirstOrDefaultAsync<User?>(sql, new { Username = username });
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

    public async Task<string?> GetRoleNameByUserIdAsync(int userId)
    {
        using var connection = CreateConnection();
        const string sql = @"
        SELECT r.Name 
        FROM Users u
        JOIN Roles r ON u.RoleId = r.Id
        WHERE u.Id = @UserId";
        return await connection.QueryFirstOrDefaultAsync<string?>(sql, new { UserId = userId });
    }
}