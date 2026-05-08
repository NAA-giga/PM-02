using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories.Interfaces;

public interface ITechCardRepository
{
    Task<IEnumerable<TechCard>> GetAllAsync();
    Task<TechCard?> GetByIdAsync(int id);
    Task<TechCardResponseDto?> GetDetailsAsync(int id);
    Task<int> CreateAsync(CreateTechCardDto dto, int userId);
    Task<bool> UpdateStatusAsync(int id, string newStatus, int userId);
    Task<bool> ApproveAsync(int id, int userId);
    Task<bool> ArchiveAsync(int id, int userId);
}
