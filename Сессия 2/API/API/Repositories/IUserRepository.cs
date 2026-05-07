using API.Models.Entities;

namespace API.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<int> CreateUserAsync(User user);
    }
}
