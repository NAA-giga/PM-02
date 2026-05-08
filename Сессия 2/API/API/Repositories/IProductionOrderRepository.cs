using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories.Interfaces;

public interface IProductionOrderRepository
{
    Task<IEnumerable<ProductionOrder>> GetAllAsync();
    Task<ProductionOrder?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateProductionOrderDto dto, int userId);
    Task<bool> UpdateStatusAsync(int id, string status, int userId);
}
