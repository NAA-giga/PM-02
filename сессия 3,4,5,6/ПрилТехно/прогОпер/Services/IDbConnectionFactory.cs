using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace прогОпер.Services
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
