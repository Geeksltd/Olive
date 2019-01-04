using Npgsql;

namespace Olive.Entities.Data
{
    public static class PostgreSqlDataAccessExtensions
    {
        public static DataAccessOptions PostgreSql(this DataAccessOptions @this)
        {
            return @this.Add<NpgsqlConnection, PostgreSqlCommandGenerator>();
        }
    }
}