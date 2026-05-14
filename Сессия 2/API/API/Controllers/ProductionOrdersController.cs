using API.Helpers;
using API.Models;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories;
using API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductionOrdersController : ControllerBase
    {
        private readonly IProductionOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IRecipeRepository _recipeRepository;
        private readonly ITechCardRepository _techCardRepository;

        public ProductionOrdersController(
            IProductionOrderRepository orderRepository,
            IProductRepository productRepository,
            IRecipeRepository recipeRepository,
            ITechCardRepository techCardRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _recipeRepository = recipeRepository;
            _techCardRepository = techCardRepository;
        }

        /// <summary>
        /// Получить список всех заказов
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderRepository.GetAllAsync();
            var dtos = new List<ProductionOrderDto>();
            foreach (var order in orders)
            {
                var dto = await _orderRepository.GetOrderDetailsAsync(order.Id);
                if (dto != null) dtos.Add(dto);
            }
            return Ok(new ApiResponse<IEnumerable<ProductionOrderDto>> { IsSuccess = true, Data = dtos });
        }

        /// <summary>
        /// Получить заказ по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderRepository.GetOrderDetailsAsync(id);
            if (order == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Заказ не найден" });
            return Ok(new ApiResponse<ProductionOrderDto> { IsSuccess = true, Data = order });
        }

        /// <summary>
        /// Создать новый заказ (статус draft)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductionOrderDto dto)
        {
            // Проверка уникальности номера заказа
            if (!await _orderRepository.IsOrderNumberUniqueAsync(dto.OrderNumber))
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Заказ с таким номером уже существует" });

            // Проверка, что продукт, рецептура и техкарта существуют
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Продукт не найден" });

            var recipe = await _recipeRepository.GetByIdAsync(dto.RecipeId);
            if (recipe == null)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Рецептура не найдена" });
            if (recipe.Status != "approved")
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Рецептура должна быть утверждена" });

            var techCard = await _techCardRepository.GetByIdAsync(dto.TechCardId);
            if (techCard == null)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Технологическая карта не найдена" });
            if (techCard.Status != "approved")
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Технологическая карта должна быть утверждена" });

            var userId = User.GetUserId();
            var order = new ProductionOrder
            {
                OrderNumber = dto.OrderNumber,
                ProductId = dto.ProductId,
                RecipeId = dto.RecipeId,
                TechCardId = dto.TechCardId,
                PlannedQuantityKg = dto.PlannedQuantityKg,
                Status = "draft",
                PlannedStartDate = dto.PlannedStartDate,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };
            var id = await _orderRepository.CreateAsync(order);
            return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { Id = id } });
        }

        /// <summary>
        /// Обновить черновик заказа (только статус draft)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductionOrderDto dto)
        {
            var existing = await _orderRepository.GetByIdAsync(id);
            if (existing == null || existing.Status != "draft")
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Редактировать можно только черновик" });

            if (!string.IsNullOrEmpty(dto.OrderNumber) && dto.OrderNumber != existing.OrderNumber)
            {
                if (!await _orderRepository.IsOrderNumberUniqueAsync(dto.OrderNumber, id))
                    return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Номер заказа уже используется" });
                existing.OrderNumber = dto.OrderNumber;
            }
            if (dto.PlannedQuantityKg.HasValue)
                existing.PlannedQuantityKg = dto.PlannedQuantityKg.Value;
            if (dto.PlannedStartDate.HasValue)
                existing.PlannedStartDate = dto.PlannedStartDate.Value;

            var success = await _orderRepository.UpdateAsync(existing);
            if (!success)
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка обновления" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        /// <summary>
        /// Изменить статус заказа (подтвердить, отменить, завершить)
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeOrderStatusDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Заказ не найден" });

            var allowedTransitions = new Dictionary<string, string[]>
            {
                { "draft", new[] { "confirmed", "cancelled" } },
                { "confirmed", new[] { "in_progress", "cancelled" } },
                { "in_progress", new[] { "completed", "cancelled" } },
                { "completed", new string[0] },
                { "cancelled", new string[0] }
            };
            if (!allowedTransitions.ContainsKey(order.Status) || !allowedTransitions[order.Status].Contains(dto.Status))
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = $"Недопустимый переход из {order.Status} в {dto.Status}" });

            // Доп. проверка: для подтверждения заказа нужно, чтобы рецептура и техкарта были утверждены
            if (dto.Status == "confirmed")
            {
                var recipe = await _recipeRepository.GetByIdAsync(order.RecipeId);
                var techCard = await _techCardRepository.GetByIdAsync(order.TechCardId);
                if (recipe?.Status != "approved")
                    return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Рецептура не утверждена" });
                if (techCard?.Status != "approved")
                    return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Технологическая карта не утверждена" });
            }

            var userId = User.GetUserId();
            var success = await _orderRepository.UpdateStatusAsync(id, dto.Status, userId);
            if (!success)
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка изменения статуса" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        /// <summary>
        /// Удалить заказ (только черновик)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _orderRepository.DeleteAsync(id);
            if (!success)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Удалить можно только черновик" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }
    }
}