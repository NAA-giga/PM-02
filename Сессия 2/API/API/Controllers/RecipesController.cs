using API.Helpers;
using API.Models;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecipesController : ControllerBase
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IProductRepository _productRepository;

    public RecipesController(IRecipeRepository recipeRepository, IProductRepository productRepository)
    {
        _recipeRepository = recipeRepository;
        _productRepository = productRepository;
    }

    /// <summary>
    /// Получить список всех рецептур
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var recipes = await _recipeRepository.GetAllAsync();
        return Ok(new ApiResponse<IEnumerable<Recipe>> { IsSuccess = true, Data = recipes });
    }

    /// <summary>
    /// Получить детальную карточку рецептуры (с составом)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var recipe = await _recipeRepository.GetRecipeDetailsAsync(id);
        if (recipe == null)
            return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Рецептура не найдена" });
        return Ok(new ApiResponse<RecipeResponseDto> { IsSuccess = true, Data = recipe });
    }

    /// <summary>
    /// Создать новую рецептуру (черновик)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecipeDto dto)
    {
        // Проверка существования продукта
        var product = await _productRepository.GetByIdAsync(dto.ProductId);
        if (product == null)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Продукт не найден" });

        var userId = User.GetUserId();
        var recipeId = await _recipeRepository.CreateAsync(dto, userId);
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { RecipeId = recipeId } });
    }

    /// <summary>
    /// Редактировать рецептуру (только черновик)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRecipeDto dto)
    {
        var recipe = await _recipeRepository.GetByIdAsync(id);
        if (recipe == null)
            return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Рецептура не найдена" });
        if (recipe.Status != "draft")
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Редактирование разрешено только для черновиков" });

        var userId = User.GetUserId();
        var updated = await _recipeRepository.UpdateAsync(id, dto, userId);
        if (!updated)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось обновить рецептуру" });

        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Рецептура обновлена" });
    }

    /// <summary>
    /// Добавить компонент в рецептуру
    /// </summary>
    [HttpPost("{recipeId}/components")]
    public async Task<IActionResult> AddComponent(int recipeId, [FromBody] RecipeComponentDto component)
    {
        var recipe = await _recipeRepository.GetByIdAsync(recipeId);
        if (recipe == null)
            return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Рецептура не найдена" });
        if (recipe.Status != "draft")
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Изменение разрешено только для черновиков" });

        var result = await _recipeRepository.AddComponentAsync(recipeId, component);
        if (!result)
            return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка при добавлении компонента" });

        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Компонент добавлен" });
    }

    /// <summary>
    /// Обновить компонент рецептуры
    /// </summary>
    [HttpPut("components/{componentId}")]
    public async Task<IActionResult> UpdateComponent(int componentId, [FromBody] RecipeComponentDto component)
    {
        var result = await _recipeRepository.UpdateComponentAsync(componentId, component);
        if (!result)
            return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Компонент не найден" });
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Компонент обновлён" });
    }

    /// <summary>
    /// Удалить компонент из рецептуры
    /// </summary>
    [HttpDelete("components/{componentId}")]
    public async Task<IActionResult> DeleteComponent(int componentId)
    {
        var result = await _recipeRepository.RemoveComponentAsync(componentId);
        if (!result)
            return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Компонент не найден" });
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Компонент удалён" });
    }

    /// <summary>
    /// Перевести рецептуру в другой статус (draft → pending → approved → archived)
    /// </summary>
    [HttpPost("{id}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] string newStatus)
    {
        var allowedTransitions = new[] { "pending", "draft" };
        if (!allowedTransitions.Contains(newStatus))
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Недопустимый статус" });

        var userId = User.GetUserId();
        var updated = await _recipeRepository.UpdateStatusAsync(id, newStatus, userId);
        if (!updated)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось изменить статус. Возможно, рецептура уже утверждена или заархивирована" });

        return Ok(new ApiResponse<object> { IsSuccess = true, Data = $"Статус изменён на {newStatus}" });
    }

    /// <summary>
    /// Утвердить рецептуру (требует 100% суммы компонентов и отсутствие другой активной рецептуры для продукта)
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var userId = User.GetUserId();
        try
        {
            var success = await _recipeRepository.ApproveAsync(id, userId);
            if (!success)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось утвердить рецептуру" });
            return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Рецептура утверждена" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = ex.Message });
        }
    }

    /// <summary>
    /// Архивировать утверждённую рецептуру
    /// </summary>
    [HttpPost("{id}/archive")]
    public async Task<IActionResult> Archive(int id)
    {
        var userId = User.GetUserId();
        var success = await _recipeRepository.ArchiveAsync(id, userId);
        if (!success)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось архивировать. Рецептура должна быть в статусе 'approved'." });
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Рецептура заархивирована" });
    }
}