using MySql.Data.MySqlClient;
using System.Data;

namespace Olive.Entities.Data
{
    public static class MySqlDataAccessExtensions
    {
        public static DataAccessOptions MySql(this DataAccessOptions @this)
        {
            return @this.Add<MySqlConnection, MySqlCommandGenerator>(new ParameterFactory());
        }
    }
}