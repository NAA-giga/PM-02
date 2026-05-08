using API.Helpers;
using API.Models;
using API.Models.DTOs;
using API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LaboratoryController : ControllerBase
{
    private readonly IProductionBatchRepository _batchRepository;
    private readonly IQualityTestRepository _testRepository;
    private readonly IEventRepository _eventRepository;

    public LaboratoryController(
        IProductionBatchRepository batchRepository,
        IQualityTestRepository testRepository,
        IEventRepository eventRepository)
    {
        _batchRepository = batchRepository;
        _testRepository = testRepository;
        _eventRepository = eventRepository;
    }

    /// <summary>
    /// Принятие лабораторного решения по партии (разрешить/заблокировать)
    /// </summary>
    [HttpPost("decide")]
    public async Task<IActionResult> Decide([FromBody] LaboratoryDecisionDto dto)
    {
        var userId = User.GetUserId();
        var batch = await _batchRepository.GetByIdAsync(dto.BatchId);
        if (batch == null)
            return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Партия не найдена" });

        // Проверяем, что все испытания завершены
        var allCompleted = await _testRepository.AreAllTestsCompletedAsync(dto.BatchId);
        if (!allCompleted)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не все испытания завершены" });

        if (dto.Decision == "approved")
        {
            await _batchRepository.UpdateLabDecisionAsync(dto.BatchId, "approved", null, userId);
            await _eventRepository.CreateEventAsync("decision", "batch", dto.BatchId,
                $"Партия {batch.BatchNumber} одобрена лабораторией", userId);
            return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Партия разрешена" });
        }
        else if (dto.Decision == "blocked")
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Причина блокировки обязательна" });
            await _batchRepository.UpdateLabDecisionAsync(dto.BatchId, "blocked", dto.Reason, userId);
            await _eventRepository.CreateEventAsync("decision", "batch", dto.BatchId,
                $"Партия {batch.BatchNumber} заблокирована: {dto.Reason}", userId);
            return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Партия заблокирована" });
        }
        else
        {
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Недопустимое решение" });
        }
    }

    /// <summary>
    /// Получить партии, ожидающие лабораторного решения
    /// </summary>
    [HttpGet("pending-batches")]
    public async Task<IActionResult> GetPendingBatches()
    {
        // Здесь можно реализовать запрос к production_batches со статусом 'quality_control'
        // Для краткости предполагаем, что есть метод в репозитории
        // var batches = await _batchRepository.GetPendingBatchesAsync();
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Метод требует реализации" });
    }
}