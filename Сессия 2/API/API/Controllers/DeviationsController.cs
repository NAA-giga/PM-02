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
public class DeviationsController : ControllerBase
{
    private readonly IDeviationRepository _deviationRepository;

    public DeviationsController(IDeviationRepository deviationRepository)
    {
        _deviationRepository = deviationRepository;
    }

    /// <summary>
    /// Получить все отклонения с фильтрацией по дате (для отчётов)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var deviations = await _deviationRepository.GetAllAsync(from, to);
        return Ok(new ApiResponse<IEnumerable<DeviationDto>> { IsSuccess = true, Data = deviations });
    }

    /// <summary>
    /// Получить отклонения по конкретной партии
    /// </summary>
    [HttpGet("batch/{batchId}")]
    public async Task<IActionResult> GetByBatchId(int batchId)
    {
        var deviations = await _deviationRepository.GetByBatchIdAsync(batchId);
        return Ok(new ApiResponse<IEnumerable<DeviationDto>> { IsSuccess = true, Data = deviations });
    }

    /// <summary>
    /// Зарегистрировать новое отклонение (аппаратчик или система)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "operator,technologist,admin")]
    public async Task<IActionResult> Create([FromBody] ReportDeviationDto dto)
    {
        if (dto == null || string.IsNullOrEmpty(dto.Description))
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Описание отклонения обязательно" });

        var userId = User.GetUserId();
        var id = await _deviationRepository.CreateAsync(dto, userId);
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { DeviationId = id } });
    }
}