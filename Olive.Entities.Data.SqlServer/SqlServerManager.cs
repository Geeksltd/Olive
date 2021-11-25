using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Olive.Entities.Data
{
    public class SqlServerManager : DatabaseServer
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

        public override void Execute(string sql, string database = null)
        {
            sql = sql.TrimOrEmpty();
            if (sql.IsEmpty()) return;

            var command = new Regex(@"^\s*GO\s*$", RegexOptions.Multiline).Split(sql).ToList();

            if (database.HasValue() && sql.Lacks("CREATE DATABASE", caseSensitive: false))
                command.Insert(0, "USE [" + database + "];");

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

        public override void ClearConnectionPool() => SqlConnection.ClearAllPools();

        public override bool Exists(string database, string filePath)
        {
            using (var connection = CreateConnection())
            {
                try { connection.Open(); }
                catch (Exception ex) { throw new Exception("Failed to open a DB connection.", ex); }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = @"SELECT [filename] FROM master.dbo.sysdatabases WHERE name='" +
                        database + "'";

                    try
                    {
                        return cmd.ExecuteScalar().ToStringOrEmpty()
                            .StartsWith(filePath, caseSensitive: false);
                    }
                    catch
                    {
                        // No logging is needed
                        return false;
                    }
                }
            }
        }
    }
}