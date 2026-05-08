using API.Helpers;
using API.Models;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductionOrdersController : ControllerBase
{
    private readonly IProductionOrderRepository _orderRepository;

    public ProductionOrdersController(IProductionOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderRepository.GetAllAsync();
        return Ok(new ApiResponse<IEnumerable<ProductionOrder>> { IsSuccess = true, Data = orders });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Заказ не найден" });
        return Ok(new ApiResponse<ProductionOrder> { IsSuccess = true, Data = order });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionOrderDto dto)
    {
        var userId = User.GetUserId();
        var orderId = await _orderRepository.CreateAsync(dto, userId);
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { OrderId = orderId } });
    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        var userId = User.GetUserId();
        var success = await _orderRepository.UpdateStatusAsync(id, "confirmed", userId);
        if (!success)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось подтвердить заказ" });
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Заказ подтверждён" });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.GetUserId();
        var success = await _orderRepository.UpdateStatusAsync(id, "cancelled", userId);
        if (!success)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось отменить заказ" });
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Заказ отменён" });
    }
}