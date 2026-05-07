namespace API.Services
{
    public interface ITokenService
    {
        string GenerateToken(string username, int userId, string role);
    }
}
