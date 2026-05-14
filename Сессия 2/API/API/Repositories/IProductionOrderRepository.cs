using API.Models.Entities;
using API.Models.DTOs;

namespace API.Repositories.Interfaces
{
    public interface IProductionOrderRepository
    {
        Task<IEnumerable<ProductionOrder>> GetAllAsync();
        Task<ProductionOrder?> GetByIdAsync(int id);
        Task<ProductionOrderDto?> GetOrderDetailsAsync(int id);
        Task<int> CreateAsync(ProductionOrder order);
        Task<bool> UpdateAsync(ProductionOrder order);
        Task<bool> UpdateStatusAsync(int id, string newStatus, int userId);
        Task<bool> DeleteAsync(int id); // мягкое удаление или просто удаление, если не связано с партиями
        Task<bool> IsOrderNumberUniqueAsync(string orderNumber, int excludeId = 0);
        Task<IEnumerable<ProductionOrder>> GetOrdersByProductAsync(int productId);
    }
}
