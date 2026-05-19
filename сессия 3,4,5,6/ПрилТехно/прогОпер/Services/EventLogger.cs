using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace прогОпер.Services
{
    public class EventLogger : IEventLogger
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public EventLogger(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task LogAsync(string eventType, string sourceType, int sourceId, string message, int? userId)
        {
            const string sql = @"
                INSERT INTO events (event_type, source_type, source_id, message, user_id, is_read, created_at)
                VALUES (@EventType, @SourceType, @SourceId, @Message, @UserId, 0, GETDATE())";

            using var conn = _connectionFactory.CreateConnection();
            await conn.ExecuteAsync(sql, new
            {
                EventType = eventType,
                SourceType = sourceType,
                SourceId = sourceId,
                Message = message,
                UserId = userId
            });
        }
    }
}
