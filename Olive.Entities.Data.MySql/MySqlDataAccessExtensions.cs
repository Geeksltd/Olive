using MySql.Data.MySqlClient;

namespace Olive.Entities.Data
{
    public static class MySqlDataAccessExtensions
    {
        public static DataAccessOptions SqlServer(this DataAccessOptions @this)
        {
            return @this.Add<MySqlConnection, MySqlCommandGenerator>();
        }
    }
}