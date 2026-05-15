using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Helpers;
using API.Models;
using API.Models.DTOs;
using API.Repositories.Interfaces;

namespace API.Controllers
{
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
        /// Получить все события с фильтрацией (для отчётов и администрирования)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "admin,technologist")]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? sourceType)
        {
            var events = await _eventRepository.GetAllAsync(from, to, sourceType);
            return Ok(new ApiResponse<IEnumerable<EventDto>> { IsSuccess = true, Data = events });
        }

        /// <summary>
        /// Получить непрочитанные события для текущего пользователя (уведомления)
        /// </summary>
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var userId = User.GetUserId();
            var events = await _eventRepository.GetUnreadEventsAsync(userId);
            return Ok(new ApiResponse<IEnumerable<EventDto>> { IsSuccess = true, Data = events });
        }

        /// <summary>
        /// Отметить событие как прочитанное
        /// </summary>
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.GetUserId();
            var success = await _eventRepository.MarkAsReadAsync(id, userId);
            if (!success)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Событие не найдено или уже прочитано" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        /// <summary>
        /// Создать событие (можно вызывать из других контроллеров или административно)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin,technologist")]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.EventType) || string.IsNullOrEmpty(dto.Message))
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не заполнены обязательные поля" });
            var id = await _eventRepository.CreateEventAsync(dto);
            return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { Id = id } });
        }
    }
}