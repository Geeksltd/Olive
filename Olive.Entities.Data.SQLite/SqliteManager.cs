using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    public class SqLiteManager : DatabaseServer
    {
        IDatabaseProviderConfig ProviderConfig;

        public SqLiteManager(IDatabaseProviderConfig providerConfig) => ProviderConfig = providerConfig;

        SqliteConnection CreateConnection() => new SqliteConnection(DataAccess.GetCurrentConnectionString());

        public override void Delete(string databaseName)
        {
            Task<IDataReader> read()
            {
                return new DataAccess<SqliteConnection>(ProviderConfig, new SqliteCommandGenerator())
                .ExecuteReader("SELECT NAME FROM sqlite_master where type = 'table'");
            }

            try
            {
                var tables = new List<string>();

                using (var reader = Task.Factory.RunSync(read))
                    while (reader.Read()) tables.Add(reader[0].ToString());

                foreach (var table in tables) Execute("DROP TABLE `" + table + "`");
            }
            catch (Exception ex)
            { throw new Exception("Could not drop database '" + databaseName + "'", ex); }
        }

        public override void Execute(string sql, string database = null)
        {
            sql = sql.TrimOrEmpty();
            if (sql.IsEmpty()) return;
            if (sql.Contains("CREATE DATABASE", caseSensitive: false)) return; // not supported

            using (var connection = CreateConnection())
            {
                try { connection.Open(); }
                catch (Exception ex) { throw new Exception("Failed to open a DB connection.", ex); }

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = sql;
                    try { cmd.ExecuteNonQuery(); }
                    catch (Exception ex) { throw new Exception("Failed to run SQL command: " + sql, ex); }
                }
            }
        }

        public override void ClearConnectionPool() { }

        public override bool Exists(string database, string filePath) => false;
    }
}