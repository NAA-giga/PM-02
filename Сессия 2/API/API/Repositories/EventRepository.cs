using Dapper;
using API.Models.Entities;
using API.Models.DTOs;
using API.Repositories.Interfaces;

namespace API.Repositories
{
    public class EventRepository : BaseRepository, IEventRepository
    {
        public EventRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<int> CreateEventAsync(CreateEventDto dto)
        {
            using var conn = CreateConnection();
            const string sql = @"
                INSERT INTO events (event_type, source_type, source_id, message, user_id, is_read, created_at)
                VALUES (@EventType, @SourceType, @SourceId, @Message, @UserId, 0, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT)";
            return await conn.ExecuteScalarAsync<int>(sql, dto);
        }

        public async Task<IEnumerable<EventDto>> GetUnreadEventsAsync(int userId)
        {
            using var conn = CreateConnection();
            const string sql = @"
                SELECT e.*, u.full_name AS UserName
                FROM events e
                LEFT JOIN users u ON e.user_id = u.id
                WHERE e.user_id = @UserId AND e.is_read = 0
                ORDER BY e.created_at DESC";
            return await conn.QueryAsync<EventDto>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<EventDto>> GetAllAsync(DateTime? from = null, DateTime? to = null, string? sourceType = null)
        {
            using var conn = CreateConnection();
            var sql = @"
                SELECT e.*, u.full_name AS UserName
                FROM events e
                LEFT JOIN users u ON e.user_id = u.id
                WHERE 1=1";
            if (from.HasValue)
                sql += " AND e.created_at >= @From";
            if (to.HasValue)
                sql += " AND e.created_at <= @To";
            if (!string.IsNullOrEmpty(sourceType))
                sql += " AND e.source_type = @SourceType";
            sql += " ORDER BY e.created_at DESC";
            return await conn.QueryAsync<EventDto>(sql, new { From = from, To = to, SourceType = sourceType });
        }

        public async Task<bool> MarkAsReadAsync(int eventId, int userId)
        {
            using var conn = CreateConnection();
            const string sql = "UPDATE events SET is_read = 1 WHERE id = @EventId AND user_id = @UserId";
            var rows = await conn.ExecuteAsync(sql, new { EventId = eventId, UserId = userId });
            return rows > 0;
        }

        public async Task<bool> DeleteOldEventsAsync(DateTime olderThan)
        {
            using var conn = CreateConnection();
            const string sql = "DELETE FROM events WHERE created_at < @OlderThan";
            var rows = await conn.ExecuteAsync(sql, new { OlderThan = olderThan });
            return rows > 0;
        }
    }
}