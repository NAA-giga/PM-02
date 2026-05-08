using API.Models.Entities;

namespace API.Repositories.Interfaces;

public interface IEventRepository
{
    Task<int> CreateEventAsync(string eventType, string sourceType, int sourceId, string message, int? userId);
    Task<IEnumerable<Event>> GetUnreadEventsAsync(int userId);
    Task MarkAsReadAsync(int eventId);
}
