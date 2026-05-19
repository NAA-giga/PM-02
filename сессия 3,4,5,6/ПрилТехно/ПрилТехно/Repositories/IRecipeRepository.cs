using System;
using System.Collections.Generic;
using System.Text;
using ПрилТехно.Models;

namespace ПрилТехно.Repositories
{
    public interface IRecipeRepository
    {
        Task<List<RecipeDto>> GetAllRecipesAsync();
        Task<RecipeDto?> GetRecipeByIdAsync(int id);
        Task<int> CreateRecipeAsync(RecipeDto recipe);
        Task<bool> UpdateRecipeAsync(RecipeDto recipe);
        Task<bool> ApproveRecipeAsync(int recipeId, int userId);
        Task<bool> ArchiveRecipeAsync(int recipeId);
        Task<bool> DeleteComponentAsync(int componentId);
    }
}
