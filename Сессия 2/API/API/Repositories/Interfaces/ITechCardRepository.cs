using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories.Interfaces
{
    public interface ITechCardRepository
    {
        // Существующие методы (должны быть)
        Task<IEnumerable<TechCard>> GetAllAsync(int? productId = null, string? status = null);
        Task<TechCard?> GetByIdAsync(int id);
        Task<TechCardDto?> GetTechCardDetailsAsync(int id);  // новый
        Task<int> CreateAsync(CreateTechCardDto dto, int userId);
        Task<bool> UpdateAsync(TechCard card);
        Task<bool> UpdateStatusAsync(int id, string newStatus, int userId);
        Task<bool> ApproveAsync(int id, int userId);
        Task<bool> ArchiveAsync(int id, int userId);
        Task<int> GetMaxVersionForProductAsync(int productId);  // новый

        // Методы для работы с шагами
        Task<bool> AddStepAsync(TechStep step);
        Task<bool> UpdateStepAsync(TechStep step);
        Task<bool> DeleteStepAsync(int stepId);
        Task<TechStep?> GetStepByIdAsync(int stepId);
    }
}