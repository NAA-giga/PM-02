using System.Data;

namespace ПрилТехно.Services
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}