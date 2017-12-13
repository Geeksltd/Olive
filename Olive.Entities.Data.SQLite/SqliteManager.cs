using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;
using System.Data;

namespace Olive.Entities.Data.SQLite
{
    public class SqliteManager
    {
        readonly string SQLiteDBLocation;
        public SqliteManager() => SQLiteDBLocation = Config.GetConnectionString("SQLiteConnection");
        public SqliteManager(string sqliteConnection) => SQLiteDBLocation = sqliteConnection;

        public void ExecuteSql(string sql)
        {
            var lines = new Regex(@"^\s*GO\s*$", RegexOptions.Multiline).Split(sql);
            using (var connection = new SqliteConnection(SQLiteDBLocation))
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

                    foreach (var line in lines.Trim())
                    {
                        cmd.CommandText = line;

                        try { cmd.ExecuteNonQuery(); }
                        catch (Exception ex) { throw EnrichError(ex, line); }
                    }
                }
            }



        }
        Exception EnrichError(Exception ex, string command) =>
           throw new Exception($"Could not execute SQL command: \r\n-----------------------\r\n{command.Trim()}\r\n-----------------------\r\n Because:\r\n\r\n{ex.Message}");
        public void DeleteDatabase()
        {
            using (var connection = new SqliteConnection(SQLiteDBLocation))
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
                    cmd.CommandText = "VACUUM";
                    try { cmd.ExecuteNonQuery(); }
                    catch (Exception ex) { throw EnrichError(ex, cmd.CommandText); }
                    
                }
            }
        }


    }
}
