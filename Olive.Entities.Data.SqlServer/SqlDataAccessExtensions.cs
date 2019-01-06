namespace Olive.Entities.Data
{
    public static class SqlDataAccessExtensions
    {
        public static DataAccessOptions SqlServer(this DataAccessOptions @this)
        {
            return @this.Add<System.Data.SqlClient.SqlConnection, SqlServerSqlCommandGenerator>();
        }
    }
}