using System;
using System.Linq;

namespace Olive.Entities.Data
{
    public interface IDatabaseServer
    {
        string GetDatabaseName();
        string GetDataSource();
        bool Exists(string tempDatabaseName, string fullName);
        void ClearConnectionPool();
        void Delete(string database);
        void Execute(string script, string database = null);
    }

    public abstract class DatabaseServer : IDatabaseServer
    {
        public abstract void Delete(string database);

        public abstract void Execute(string script, string database = null);

        public abstract bool Exists(string database, string filePath);

        public abstract void ClearConnectionPool();

        public string GetDataSource() => GetConnectionStringSetting("data source", "server");

        public string GetDatabaseName() => GetConnectionStringSetting("initial catalog", "database");

        string GetConnectionStringSetting(params string[] names)
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