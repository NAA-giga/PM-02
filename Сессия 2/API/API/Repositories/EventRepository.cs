using Dapper;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Repositories;

public class EventRepository : BaseRepository, IEventRepository
{
    public EventRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<int> CreateEventAsync(string eventType, string sourceType, int sourceId, string message, int? userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
            INSERT INTO events (event_type, source_type, source_id, message, user_id, is_read, created_at)
            VALUES (@EventType, @SourceType, @SourceId, @Message, @UserId, 0, GETDATE());
            SELECT CAST(SCOPE_IDENTITY() AS INT)";
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            EventType = eventType,
            SourceType = sourceType,
            SourceId = sourceId,
            Message = message,
            UserId = userId
        });
    }

    public async Task<IEnumerable<Event>> GetUnreadEventsAsync(int userId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM events WHERE user_id = @UserId AND is_read = 0 ORDER BY created_at DESC";
        return await conn.QueryAsync<Event>(sql, new { UserId = userId });
    }

    public async Task MarkAsReadAsync(int eventId)
    {
        using var conn = CreateConnection();
        await conn.ExecuteAsync("UPDATE events SET is_read = 1 WHERE id = @Id", new { Id = eventId });
    }
}