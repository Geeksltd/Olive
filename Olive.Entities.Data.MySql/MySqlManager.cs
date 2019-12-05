using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Olive.Entities.Data.MySql
{
    public class MySqlManager : DatabaseServer
    {
        public override void ClearConnectionPool() => MySqlConnection.ClearAllPools();

        public override void Delete(string database)
        {
            var script = $"DROP DATABASE IF EXISTS `{database}`;";

            try { Execute(script); }
            catch (Exception ex)
            { throw new Exception("Could not drop database '" + database + "'", ex); }
        }

        public override void Execute(string script, string database = null)
        {
            script = script.TrimOrEmpty();
            if (script.IsEmpty()) return;

            var command = new Regex(@"^\s*GO\s*$", RegexOptions.Multiline).Split(script).ToList();

            if (database.HasValue() && script.Lacks("CREATE DATABASE", caseSensitive: false))
                command.Insert(0, "USE `" + database + "`;");

            using (var connection = CreateConnection())
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to open a DB connection.", ex);
                }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;

                    foreach (var line in command.Trim())
                    {
                        cmd.CommandText = line;

                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Failed to run SQL command: " + line, ex);
                        }
                    }
                }
            }
        }

        public override bool Exists(string database, string filePath)
        {
            using (var connection = CreateConnection())
            {
                try { connection.Open(); }
                catch (Exception ex) { throw new Exception("Failed to open a DB connection.", ex); }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = $"SELECT Count(*) FROM information_schema.tables WHERE TABLE_SCHEMA = '{database}'";

                    try
                    {
                        return Convert.ToInt64(cmd.ExecuteScalar()) > 0 &&
                            System.IO.Directory.Exists(filePath);
                    }
                    catch
                    {
                        // No logging is needed
                        return false;
                    }
                }
            }
        }

        MySqlConnection CreateConnection()
        {
            var connectionString = new MySqlConnectionStringBuilder(DataAccess.GetCurrentConnectionString())
            {
                Database = "sys"
            }.ToString();

            return new MySqlConnection(connectionString);
        }
    }
}