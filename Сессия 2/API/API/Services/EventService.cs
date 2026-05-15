using API.Models.DTOs;
using API.Repositories.Interfaces;

namespace API.Services
{
    public interface IEventService
    {
        Task LogEventAsync(string eventType, string sourceType, int sourceId, string message, int? userId = null);
    }

    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task LogEventAsync(string eventType, string sourceType, int sourceId, string message, int? userId = null)
        {
            var dto = new CreateEventDto
            {
                EventType = eventType,
                SourceType = sourceType,
                SourceId = sourceId,
                Message = message,
                UserId = userId
            };
            await _eventRepository.CreateEventAsync(dto);
        }
    }
}