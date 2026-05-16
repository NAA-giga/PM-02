using API.Helpers;
using API.Models;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IUserRepository _userRepository;

        public RecipesController(IRecipeRepository recipeRepository, IUserRepository userRepository)
        {
            _recipeRepository = recipeRepository;
            _userRepository = userRepository;
        }

        // GET: api/recipes
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? productId, [FromQuery] string? status)
        {
            var recipes = await _recipeRepository.GetAllAsync(productId, status);
            var dtos = recipes.Select(r => new RecipeDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                Version = r.Version,
                Name = r.Name,
                Status = r.Status,
                ApprovedAt = r.ApprovedAt,
                ApprovedBy = r.ApprovedBy,
                CreatedBy = r.CreatedBy,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            });
            return Ok(new ApiResponse<IEnumerable<RecipeDto>> { IsSuccess = true, Data = dtos });
        }

        // GET: api/recipes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var recipe = await _recipeRepository.GetRecipeDetailsAsync(id);
            if (recipe == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Рецептура не найдена" });
            return Ok(new ApiResponse<RecipeResponseDto> { IsSuccess = true, Data = recipe });
        }

        // POST: api/recipes
        [HttpPost]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Create([FromBody] CreateRecipeDto dto)
        {
            // Вычисляем следующую версию
            var maxVersion = await _recipeRepository.GetMaxVersionForProductAsync(dto.ProductId);
            var nextVersion = maxVersion + 1;

            var userId = User.GetUserId();
            var recipe = new Recipe
            {
                ProductId = dto.ProductId,
                Version = nextVersion,          // ← автоматически
                Name = dto.Name,
                Status = "draft",
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var id = await _recipeRepository.CreateAsync(recipe);
            return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { Id = id } });
        }

        // PUT: api/recipes/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRecipeDto dto)
        {
            var existing = await _recipeRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Рецептура не найдена" });
            if (existing.Status != "draft")
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Редактировать можно только черновик" });

            existing.Name = dto.Name;
            existing.UpdatedAt = DateTime.UtcNow;
            var success = await _recipeRepository.UpdateAsync(existing);
            if (!success)
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка обновления" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        // POST: api/recipes/{id}/approve
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var userId = User.GetUserId();
            try
            {
                var success = await _recipeRepository.ApproveAsync(id, userId);
                if (!success)
                    return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось утвердить" });
                return Ok(new ApiResponse<object> { IsSuccess = true });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = ex.Message });
            }
        }

        // POST: api/recipes/{id}/archive
        [HttpPost("{id}/archive")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Archive(int id)
        {
            var userId = User.GetUserId();
            var success = await _recipeRepository.ArchiveAsync(id, userId);
            if (!success)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось архивировать" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        // POST: api/recipes/{recipeId}/components
        [HttpPost("{recipeId}/components")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> AddComponent(int recipeId, [FromBody] RecipeComponentDto dto)
        {
            var recipe = await _recipeRepository.GetByIdAsync(recipeId);
            if (recipe == null || recipe.Status != "draft")
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Можно добавлять компоненты только в черновик" });

            var component = new RecipeComponent
            {
                RecipeId = recipeId,
                RawMaterialId = dto.RawMaterialId,
                Percentage = dto.Percentage,
                LoadOrder = dto.LoadOrder,
                ToleranceMin = dto.ToleranceMin ?? 0,
                ToleranceMax = dto.ToleranceMax ?? 0
            };
            var success = await _recipeRepository.AddComponentAsync(recipeId, component);
            return success
                ? Ok(new ApiResponse<object> { IsSuccess = true })
                : StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка добавления компонента" });
        }

        // PUT: api/recipes/components/{componentId}
        [HttpPut("components/{componentId}")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> UpdateComponent(int componentId, [FromBody] RecipeComponentDto dto)
        {
            var component = new RecipeComponent
            {
                Id = componentId,
                RawMaterialId = dto.RawMaterialId,
                Percentage = dto.Percentage,
                LoadOrder = dto.LoadOrder,
                ToleranceMin = dto.ToleranceMin ?? 0,
                ToleranceMax = dto.ToleranceMax ?? 0
            };
            var success = await _recipeRepository.UpdateComponentAsync(component);
            return success
                ? Ok(new ApiResponse<object> { IsSuccess = true })
                : NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Компонент не найден" });
        }

        // DELETE: api/recipes/components/{componentId}
        [HttpDelete("components/{componentId}")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> DeleteComponent(int componentId)
        {
            var success = await _recipeRepository.RemoveComponentAsync(componentId);
            return success
                ? Ok(new ApiResponse<object> { IsSuccess = true })
                : NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Компонент не найден" });
        }
    }
}