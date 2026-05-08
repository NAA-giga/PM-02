using API.Models.DTOs;
using API.Models.Entities;

namespace API.Repositories;

public interface IRecipeRepository
{
    Task<IEnumerable<Recipe>> GetAllAsync(int? productId = null, string? status = null);
    Task<IEnumerable<Recipe>> GetAllAsync();
    Task<Recipe?> GetByIdAsync(int id);
    Task<RecipeResponseDto?> GetRecipeDetailsAsync(int id);
    Task<int> CreateAsync(CreateRecipeDto dto, int userId);
    Task<bool> UpdateAsync(int id, UpdateRecipeDto dto, int userId);
    Task<bool> AddComponentAsync(int recipeId, RecipeComponentDto component);
    Task<bool> UpdateComponentAsync(int componentId, RecipeComponentDto component);
    Task<bool> RemoveComponentAsync(int componentId);
    Task<bool> UpdateStatusAsync(int id, string newStatus, int userId);
    Task<bool> ApproveAsync(int id, int userId);
    Task<bool> ArchiveAsync(int id, int userId);
    Task<decimal> GetTotalPercentageAsync(int recipeId);
    Task<bool> IsAnyApprovedForProductAsync(int productId, int excludeRecipeId = 0);
}