using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;
using Dapper;

namespace API.Repositories;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var connection = CreateConnection();
        const string sql = @"
        SELECT 
            id,
            username,
            password_hash AS PasswordHash,
            full_name AS FullName,
            email,
            role_id AS RoleId,
            department_id AS DepartmentId,
            is_active AS IsActive,
            created_at AS CreatedAt,
            last_login AS LastLogin,
            photo
        FROM users
        WHERE username = @Username";

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

    public async Task<string?> GetRoleNameByUserIdAsync(int userId)
    {
        using var connection = CreateConnection();
        const string sql = @"
        SELECT r.Name
        FROM users u
        JOIN roles r ON u.role_id = r.id
        WHERE u.id = @UserId";
        return await connection.QueryFirstOrDefaultAsync<string?>(sql, new { UserId = userId });
    }
    public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
        SELECT 
            u.id AS Id,
            u.username AS Username,
            u.full_name AS FullName,
            u.email AS Email,
            r.name AS Role,
            d.name AS Department,
            u.photo AS Photo
        FROM users u
        JOIN roles r ON u.role_id = r.id
        JOIN departments d ON u.department_id = d.id
        WHERE u.id = @UserId";

        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(sql, new { UserId = userId });
        if (result == null) return null;

        return new UserProfileDto
        {
            Id = result.Id != null ? Convert.ToInt32(result.Id) : 0,
            Username = result.Username?.ToString() ?? string.Empty,
            FullName = result.FullName?.ToString() ?? string.Empty,
            Email = result.Email?.ToString() ?? string.Empty,
            Role = result.Role?.ToString() ?? string.Empty,
            Department = result.Department?.ToString() ?? string.Empty,
            PhotoBase64 = result.Photo != null ? Convert.ToBase64String((byte[])result.Photo) : null
        };
    }
}