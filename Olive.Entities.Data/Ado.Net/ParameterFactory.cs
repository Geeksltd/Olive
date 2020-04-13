using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Olive.Entities
{
    public interface IParameterFactory
    {
        IDataParameter CreateParameter(string name, object value, DbType? dbType);
    }

    internal class DefaultParameterFactory<TConnection> : IParameterFactory
        where TConnection : DbConnection, new()
    {
        readonly DbCommand Command;

        public DefaultParameterFactory()
        {
            Command = new TConnection().CreateCommand();
        }

        public IDataParameter CreateParameter(string name, object value, DbType? dbType)
        {
            var result = Command.CreateParameter();

            result.ParameterName = name;
            result.Value = value;

            if (dbType.HasValue) result.DbType = dbType.Value;

            return result;
        }
    }
}
