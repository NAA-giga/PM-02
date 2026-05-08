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
public class EventsController : ControllerBase
{
    private readonly IEventRepository _eventRepository;

    public EventsController(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    /// <summary>
    /// Получить непрочитанные уведомления для текущего пользователя
    /// </summary>
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var userId = User.GetUserId();
        var events = await _eventRepository.GetUnreadEventsAsync(userId);
        var dtos = events.Select(e => new EventResponseDto
        {
            Id = e.Id,
            EventType = e.EventType,
            SourceType = e.SourceType,
            SourceId = e.SourceId,
            Message = e.Message,
            IsRead = e.IsRead,
            CreatedAt = e.CreatedAt
        });
        return Ok(new ApiResponse<IEnumerable<EventResponseDto>> { IsSuccess = true, Data = dtos });
    }

    /// <summary>
    /// Отметить уведомление как прочитанное
    /// </summary>
    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _eventRepository.MarkAsReadAsync(id);
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Уведомление отмечено прочитанным" });
    }
}