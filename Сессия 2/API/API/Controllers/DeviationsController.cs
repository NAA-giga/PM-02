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
public class DeviationsController : ControllerBase
{
    private readonly IDeviationRepository _deviationRepository;
    private readonly IEventRepository _eventRepository;

    public DeviationsController(IDeviationRepository deviationRepository, IEventRepository eventRepository)
    {
        _deviationRepository = deviationRepository;
        _eventRepository = eventRepository;
    }

    /// <summary>
    /// Сообщить об отклонении (аппаратчик или система)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Report([FromBody] ReportDeviationDto dto)
    {
        var userId = User.GetUserId();
        var devId = await _deviationRepository.CreateAsync(dto, userId);
        await _eventRepository.CreateEventAsync(new CreateEventDto
        {
            EventType = "deviation",
            SourceType = "batch",
            SourceId = dto.ProductionBatchId,
            Message = dto.Description,
            UserId = userId
        });
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { DeviationId = devId } });
    }

    /// <summary>
    /// Получить все отклонения по партии
    /// </summary>
    [HttpGet("batch/{batchId}")]
    public async Task<IActionResult> GetByBatch(int batchId)
    {
        var deviations = await _deviationRepository.GetByBatchIdAsync(batchId);
        return Ok(new ApiResponse<IEnumerable<Deviation>> { IsSuccess = true, Data = deviations });
    }
}