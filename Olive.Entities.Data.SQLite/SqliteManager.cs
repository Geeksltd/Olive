using System;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace Olive.Entities.Data
{
    public class SqLiteManager : DatabaseManager
    {
        SqliteConnection CreateConnection() => new SqliteConnection(DataAccess.GetCurrentConnectionString());

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

        public override void ClearConnectionPool() { }
    }
}