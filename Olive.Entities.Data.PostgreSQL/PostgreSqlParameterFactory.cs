using System;
using System.Data;
using Npgsql;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Olive tags every DateTime parameter as DbType.DateTime2, which Npgsql writes as
    /// "timestamp without time zone" and refuses for a DateTime whose Kind is Utc. SQL Server stored such
    /// a value as it was, ignoring its Kind, so the same is done here: the Kind is dropped and the
    /// clock time kept, with no conversion.
    /// </summary>
    public class PostgreSqlParameterFactory : IParameterFactory
    {
        public IDataParameter CreateParameter(string name, object value, DbType? dbType)
        {
            if (value is DateTime time && time.Kind != DateTimeKind.Unspecified)
                value = DateTime.SpecifyKind(time, DateTimeKind.Unspecified);

            var result = new NpgsqlParameter { ParameterName = name, Value = value };
            if (dbType.HasValue) result.DbType = dbType.Value;
            return result;
        }
    }
}
