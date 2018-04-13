using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Olive.Entities.Data
{
    public class SqlServerManager : DatabaseManager
    {
        SqlConnection CreateConnection()
        {
            var connectionString = new SqlConnectionStringBuilder(DataAccess.GetCurrentConnectionString())
            {
                InitialCatalog = "master"
            }.ToString();

            return new SqlConnection(connectionString);
        }

        public override void Delete(string databaseName)
        {
            var script = @"
IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'{0}')
BEGIN
    ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    ALTER DATABASE [{0}] SET MULTI_USER;
    DROP DATABASE [{0}];
END".FormatWith(databaseName);

            try { Execute(script); }
            catch (Exception ex)
            { throw new Exception("Could not drop database '" + databaseName + "'", ex); }
        }

        public override void Execute(string sql)
        {
            var command = new Regex(@"^\s*GO\s*$", RegexOptions.Multiline).Split(sql);

            using (var connection = CreateConnection())
            {
                try { connection.Open(); }
                catch (Exception ex) { throw new Exception("Failed to open a DB connection.", ex); }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;

                    foreach (var line in command.Trim())
                    {
                        cmd.CommandText = line;

                        try { cmd.ExecuteNonQuery(); }
                        catch (Exception ex) { throw new Exception("Failed to run SQL command: " + line, ex); }
                    }
                }
            }
        }

        public void Detach(string databaseName)
        {
            var script = @"
ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
ALTER DATABASE [{0}] SET MULTI_USER;
exec sp_detach_db '{0}'".FormatWith(databaseName);

            try { Execute(script); }
            catch (Exception ex)
            { throw new Exception($"Could not detach database '{databaseName}'.", ex); }
        }

        //public bool Exists(string databaseName)
        //{
        //    var script = $"SELECT count(name) FROM master.dbo.sysdatabases WHERE name = N'{databaseName}'";

        //    using (var connection = CreateConnection())
        //    {
        //        connection.Open();

        //        using (var cmd = connection.CreateCommand())
        //        {
        //            cmd.CommandType = CommandType.Text;
        //            cmd.CommandText = script;

        //            bool result;
        //            try { result = (int)cmd.ExecuteScalar() > 0; }
        //            catch (Exception ex) { throw new Exception("Failed to run sql: " + script, ex); }

        //            Debug.WriteLine($"Database '{databaseName}' already exists in '{connection.DataSource}' -> " + result);
        //            return result;
        //        }
        //    }
        //}

        public override void ClearConnectionPool() => SqlConnection.ClearAllPools();
    }
}