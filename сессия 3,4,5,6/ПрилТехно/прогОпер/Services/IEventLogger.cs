using System;
using System.Collections.Generic;
using System.Text;

namespace прогОпер.Services
{
    public interface IEventLogger
    {
        Task LogAsync(string eventType, string sourceType, int sourceId, string message, int? userId);
    }
}
