using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Olive.Entities.Data
{
    class ParameterFactory : IParameterFactory
    {
        public IDataParameter CreateParameter(string name, object value, DbType? dbType) =>
            new MySqlParameter(name, value);
    }
}
