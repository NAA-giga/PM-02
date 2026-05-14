using API.Models.Entities;
namespace API.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<int> CreateAsync(Product product);               // новый
        Task<bool> UpdateAsync(Product product);             // новый
        Task<bool> ArchiveAsync(int id);                     // новый
    }
}
