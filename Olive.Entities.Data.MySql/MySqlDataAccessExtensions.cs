using MySql.Data.MySqlClient;

namespace Olive.Entities.Data
{
    public static class MySqlDataAccessExtensions
    {
        public static DataAccessOptions MySql(this DataAccessOptions @this)
        {
            return @this.Add<MySqlConnection, MySqlCommandGenerator>();
        }
    }
}