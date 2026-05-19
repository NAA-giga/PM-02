using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
namespace ПрогЛабор.Services
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
