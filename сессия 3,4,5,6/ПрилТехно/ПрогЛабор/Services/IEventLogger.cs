using System;
using System.Collections.Generic;
using System.Text;

namespace ПрогЛабор.Services
{
    public interface IEventLogger
    {
        Task LogAsync(string eventType, string sourceType, int sourceId, string message, int? userId);
    }
}
