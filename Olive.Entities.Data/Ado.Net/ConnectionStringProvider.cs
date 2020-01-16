using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Entities.Data
{
    public interface IConnectionStringProvider
    {
        string GetConnectionString(string key = "Default");
    }

    public class ConnectionStringProvider : IConnectionStringProvider
    {
        public virtual string GetConnectionString(string key = "Default") => Config.GetOrThrow($"ConnectionStrings:{key}");
    }
}
