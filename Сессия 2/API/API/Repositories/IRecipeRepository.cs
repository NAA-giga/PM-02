using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories;

public interface IRecipeRepository
{
    Task<IEnumerable<Recipe>> GetAllAsync(int? productId = null, string? status = null);
    Task<Recipe?> GetByIdAsync(int id);
    Task<RecipeResponseDto?> GetRecipeDetailsAsync(int id); // с компонентами
    Task<int> CreateAsync(Recipe recipe);
    Task<bool> UpdateAsync(Recipe recipe);
    Task<bool> UpdateStatusAsync(int id, string newStatus, int userId);
    Task<bool> ApproveAsync(int id, int userId);
    Task<bool> ArchiveAsync(int id, int userId);
    Task<bool> AddComponentAsync(int recipeId, RecipeComponent component);
    Task<bool> UpdateComponentAsync(RecipeComponent component);
    Task<bool> RemoveComponentAsync(int componentId);
    Task<decimal> GetTotalPercentageAsync(int recipeId);
    Task<bool> IsAnyApprovedForProductAsync(int productId, int excludeRecipeId = 0);
    Task<int> GetMaxVersionForProductAsync(int productId);
}