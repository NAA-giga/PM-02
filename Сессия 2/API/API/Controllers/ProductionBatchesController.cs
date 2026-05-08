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
public class ProductionBatchesController : ControllerBase
{
    private readonly IProductionBatchRepository _batchRepository;
    private readonly IBatchStepExecutionRepository _stepRepository;

    public ProductionBatchesController(
        IProductionBatchRepository batchRepository,
        IBatchStepExecutionRepository stepRepository)
    {
        _batchRepository = batchRepository;
        _stepRepository = stepRepository;
    }

    /// <summary>
    /// Список активных партий (для аппаратчика)
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var batches = await _batchRepository.GetActiveBatchesAsync();
        return Ok(new ApiResponse<IEnumerable<ProductionBatch>> { IsSuccess = true, Data = batches });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var batch = await _batchRepository.GetBatchWithStepsAsync(id);
        if (batch == null)
            return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Партия не найдена" });
        return Ok(new ApiResponse<ProductionBatchResponseDto> { IsSuccess = true, Data = batch });
    }

    /// <summary>
    /// Запуск новой партии на основе заказа
    /// </summary>
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartProductionBatchDto dto)
    {
        var userId = User.GetUserId();
        try
        {
            var batchId = await _batchRepository.StartBatchAsync(dto, userId);
            return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { BatchId = batchId } });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = ex.Message });
        }
    }

    /// <summary>
    /// Начать выполнение шага
    /// </summary>
    [HttpPost("{batchId}/steps/{stepOrder}/start")]
    public async Task<IActionResult> StartStep(int batchId, int stepOrder)
    {
        var userId = User.GetUserId();
        var success = await _stepRepository.StartStepAsync(batchId, stepOrder, userId);
        if (!success)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось начать шаг" });
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Шаг начат" });
    }

    /// <summary>
    /// Завершить шаг и ввести фактические параметры
    /// </summary>
    [HttpPost("{batchId}/steps/{stepOrder}/complete")]
    public async Task<IActionResult> CompleteStep(int batchId, int stepOrder, [FromBody] PerformStepDto data)
    {
        var userId = User.GetUserId();
        var success = await _stepRepository.CompleteStepAsync(batchId, stepOrder, data, userId);
        if (!success)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось завершить шаг" });
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Шаг завершён" });
    }

    /// <summary>
    /// Завершить партию (если все шаги выполнены)
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteBatch(int id, [FromBody] decimal actualQuantity)
    {
        var userId = User.GetUserId();
        var success = await _batchRepository.CompleteBatchAsync(id, actualQuantity, userId);
        if (!success)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось завершить партию" });
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Партия завершена" });
    }
}