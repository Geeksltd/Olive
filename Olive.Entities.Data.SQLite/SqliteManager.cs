using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Olive.Entities.Data.SQLite
{
    public class SqliteManager
    {
        readonly string SQLiteDBLocation;
        public SqliteManager() => SQLiteDBLocation = Config.GetConnectionString("SQLiteDBLocation");

        public void ExecuteSql(string sql)
        {
            using (var db = new SQLiteConnection(SQLiteDBLocation))
            {
                try
                {
                    db.Execute(sql);
                }
                catch (Exception ex) { throw EnrichError(ex, sql); }
            }
        }
        public List<T> ExecuteSql<T>(string sql) where T : class, new()
        {
            using (var db = new SQLiteConnection(SQLiteDBLocation))
            {
                try
                {
                    return db.Query<T>(sql);
                }
                catch (Exception ex) { throw EnrichError(ex, sql); }
            }
        }
        Exception EnrichError(Exception ex, string command) =>
           throw new Exception($"Could not execute SQL command: \r\n-----------------------\r\n{command.Trim()}\r\n-----------------------\r\n Because:\r\n\r\n{ex.Message}");
        public void DeleteDatabase()
        {
            using (var db = new SQLiteConnection(SQLiteDBLocation))
            {
                db.Execute("VACUUM");
            }
        }


    }
}
