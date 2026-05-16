using API.Models.Entities;
using API.Models.DTOs;
namespace API.Repositories.Interfaces;

public interface IEventRepository
{
    Task<int> CreateEventAsync(CreateEventDto dto);
    Task<IEnumerable<EventDto>> GetUnreadEventsAsync(int userId);
    Task<IEnumerable<EventDto>> GetAllAsync(DateTime? from = null, DateTime? to = null, string? sourceType = null);
    Task<bool> MarkAsReadAsync(int eventId, int userId);
    Task<bool> DeleteOldEventsAsync(DateTime olderThan);
}
