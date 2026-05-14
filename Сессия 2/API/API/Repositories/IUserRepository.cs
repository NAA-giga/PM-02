using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<int> CreateUserAsync(User user);
    Task<string?> GetRoleNameByUserIdAsync(int userId);
    Task<UserProfileDto?> GetUserProfileAsync(int userId);
}
