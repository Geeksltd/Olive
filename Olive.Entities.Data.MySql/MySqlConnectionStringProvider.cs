using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Entities.Data
{
    public class MySqlConnectionStringProvider : ConnectionStringProvider
    {
        public override string GetConnectionString(string key = "Default")
        {
            var builder = new MySqlConnectionStringBuilder(base.GetConnectionString(key))
            {
                UseAffectedRows = false
            };

            return builder.ToString();
        }
    }
}
