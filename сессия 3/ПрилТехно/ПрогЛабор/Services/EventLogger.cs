// Services/EventLogger.cs
using Dapper;
using LaboratoryApp.Services;
using System;
using System.Data;
using System.Threading.Tasks;
using ПрогЛабор.Services;

namespace LaboratoryApp.Services
{
    public class EventLogger : IEventLogger
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public EventLogger(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private IDbConnection CreateConnection() => _connectionFactory.CreateConnection();

        public async Task LogAsync(string eventType, string sourceType, int sourceId, string message, int? userId)
        {
            const string sql = @"
                INSERT INTO events (event_type, source_type, source_id, message, user_id, is_read, created_at)
                VALUES (@EventType, @SourceType, @SourceId, @Message, @UserId, 0, GETDATE())";

            using var conn = CreateConnection();
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