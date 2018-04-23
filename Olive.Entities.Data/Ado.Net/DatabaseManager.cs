using System;
using System.Linq;

namespace Olive.Entities.Data
{
    public abstract class DatabaseManager
    {
        public abstract void Delete(string database);
        public abstract void Execute(string script, string database = null);
        public abstract bool Exists(string database, string filePath);
        public abstract void ClearConnectionPool();

        public static string GetDataSource() => GetConnectionStringSetting("data source", "server");

        public static string GetDatabaseName() => GetConnectionStringSetting("initial catalog", "database");

        static string GetConnectionStringSetting(params string[] names)
        {
            var connectionString = DataAccess.GetCurrentConnectionString();
            if (connectionString.IsEmpty()) throw new Exception("There is no current connection string");

            return connectionString.KeepReplacing("  ", " ").Split(';').Trim()
                .Select(x => x.Split('=').Trim())
                 .Where(x => x.Count() == 2)
                 .Where(x => names.Contains(x.First().ToLower()))
                 .Select(x => x.Last().Trim('[', ']', '`', '\'')).Trim()
                 .FirstOrDefault();
        }
    }
}