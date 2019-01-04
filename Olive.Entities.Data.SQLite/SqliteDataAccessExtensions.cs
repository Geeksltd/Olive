using Microsoft.Data.Sqlite;

namespace Olive.Entities.Data
{
    public static class SqliteDataAccessExtensions
    {
        public static DataAccessOptions SQLite(this DataAccessOptions @this)
        {
            return @this.Add<SqliteConnection, SqliteCommandGenerator>();
        }
    }
}