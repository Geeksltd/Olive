using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Npgsql;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Builds the development temp database from M#'s T-SQL scripts, translated by
    /// <see cref="PostgreSqlScriptTranslator"/>.
    /// </summary>
    public class PostgreSqlManager : DatabaseServer
    {
        static readonly Regex CreateDatabase = new Regex(@"^\s*CREATE\s+DATABASE\b", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // SQL Server's data file path, which the temp database generator makes unique to the current
        // scripts. With no files to compare here, it is kept as the database's comment instead.
        static readonly Regex DataFile = new Regex(@"FILENAME\s*=\s*N?'(?<path>[^']+)'", RegexOptions.IgnoreCase);

        NpgsqlConnection CreateConnection(string database) =>
            new NpgsqlConnection(new NpgsqlConnectionStringBuilder(DataAccess.GetCurrentConnectionString())
            {
                Database = database.Or("postgres"),
                Pooling = false
            }.ToString());

        public override void Delete(string databaseName) =>
            Run(null, new[] { $"DROP DATABASE IF EXISTS {Quote(databaseName)} WITH (FORCE)" });

        public override void Execute(string sql, string database = null)
        {
            if (CreateDatabase.IsMatch(sql.OrEmpty()))
            {
                Create(database, DataFile.Match(sql).Groups["path"].Value);
                return;
            }

            var statements = PostgreSqlScriptTranslator.Translate(sql).ToArray();
            if (statements.None()) return;

            database = database.Or(GetDatabaseName());
            if (!Exists(database)) Create(database, marker: "");

            Run(database, statements, inTransaction: true);
        }

        void Create(string database, string marker)
        {
            Run(null, new[]
            {
                $"CREATE DATABASE {Quote(database)}",
                $"COMMENT ON DATABASE {Quote(database)} IS '{marker.Replace("'", "''")}'"
            });

            Run(database, PostgreSqlScriptTranslator.CollationSetup);
        }

        public override void ClearConnectionPool() => NpgsqlConnection.ClearAllPools();

        public override bool Exists(string database, string filePath) =>
            ReadComment(database)?.StartsWith(filePath, StringComparison.OrdinalIgnoreCase) == true;

        bool Exists(string database) => ReadComment(database) != null;

        /// <summary>Null when the database does not exist.</summary>
        string ReadComment(string database)
        {
            using (var connection = CreateConnection(null))
            {
                try { connection.Open(); }
                catch (Exception ex) { throw new Exception("Failed to open a DB connection.", ex); }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COALESCE(shobj_description(oid, 'pg_database'), '') FROM pg_database WHERE datname = @name";
                    command.Parameters.AddWithValue("name", database);
                    return command.ExecuteScalar() as string;
                }
            }
        }

        void Run(string database, IEnumerable<string> statements, bool inTransaction = false)
        {
            using (var connection = CreateConnection(database))
            {
                try { connection.Open(); }
                catch (Exception ex) { throw new Exception("Failed to open a DB connection.", ex); }

                using (var transaction = inTransaction ? connection.BeginTransaction() : null)
                {
                    foreach (var statement in statements)
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = statement;
                            command.Transaction = transaction;

                            try { command.ExecuteNonQuery(); }
                            catch (Exception ex) { throw new Exception("Failed to run SQL command: " + statement, ex); }
                        }

                    transaction?.Commit();
                }
            }
        }

        static string Quote(string name) => "\"" + name.Replace("\"", "\"\"") + "\"";
    }
}
