﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    public abstract class DataAccess
    {
        public static string GetCurrentConnectionString()
        {
            string result;

            if (DatabaseContext.Current != null) result = DatabaseContext.Current.ConnectionString;
            else result = Config.GetConnectionString("Default");

            if (result.IsEmpty())
                throw new Exception("No 'AppDatabase' connection string is specified in the application config file.");

            return result;
        }
    }

    /// <summary>
    /// ADO.NET Facade for submitting single method commands.
    /// </summary>
    public class DataAccess<TConnection> : DataAccess, IDataAccess
        where TConnection : DbConnection, new()
    {
        /// <summary>
        /// Creates a connection object.
        /// </summary>
        public async Task<IDbConnection> GetOrCreateConnection()
        {
            var result = await (DbTransactionScope.Root?.GetDbConnection() ?? Task.FromResult<IDbConnection>(null));
            if (result != null) return (TConnection)result;
            else return await CreateConnection();
        }

        /// <summary>
        /// Creates a new DB Connection to database with the given connection string.
        /// </summary>		
        public async Task<IDbConnection> CreateConnection(string connectionString = null)
        {
            var result = new TConnection
            {
                ConnectionString = connectionString.Or(GetCurrentConnectionString())
            };

            await result.OpenAsync();
            return result;
        }

        void CloseConnection(IDbConnection connection)
        {
            if (DbTransactionScope.Root == null)
            {
                if (connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        async Task<DbCommand> CreateCommand(CommandType type, string commandText, params IDataParameter[] @params) =>
            await CreateCommand(type, commandText, default(TConnection), @params);

        async Task<DbCommand> CreateCommand(CommandType type, string commandText, IDbConnection connection, params IDataParameter[] @params)
        {
            if (connection == null) connection = await GetOrCreateConnection();

            var command = (DbCommand)connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = type;

            command.Transaction = await (DbTransactionScope.Root?.GetDbTransaction()
                ?? Task.FromResult(command.Transaction));

            command.CommandTimeout = DatabaseContext.Current?.CommandTimeout ?? (Config.TryGet<int?>("Sql.Command.TimeOut")) ?? command.CommandTimeout;

            foreach (var param in @params)
                command.Parameters.Add(param);

            return command;
        }

        DataAccessProfiler.Watch StartWatch(string command)
        {
            if (Database.Configuration.Profile) return DataAccessProfiler.Start(command);
            else return null;
        }

        /// <summary>
        /// Executes the specified command text as nonquery.
        /// </summary>
        public async Task<int> ExecuteNonQuery(string command, CommandType commandType = CommandType.Text, params IDataParameter[] @params)
        {
            var dbCommand = await CreateCommand(commandType, command, @params);

            var watch = StartWatch(command);

            try
            {
                var result = await dbCommand.ExecuteNonQueryAsync();
                DatabaseStateChangeCommand.Raise(command, commandType, @params);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in running Non-Query SQL command.", ex).AddData("Command", command)
                    .AddData("Parameters", @params?.Select(p => p.ParameterName + "=" + p.Value).ToString(" | "))
                    .AddData("ConnectionString", dbCommand.Connection.ConnectionString);
            }
            finally
            {
                dbCommand.Parameters.Clear();

                CloseConnection(dbCommand.Connection);

                if (watch != null) DataAccessProfiler.Complete(watch);
            }
        }

        /// <summary>
        /// Executes the specified command text against the database connection of the context and builds an IDataReader.
        /// Make sure you close the data reader after finishing the work.
        /// </summary>
        public async Task<IDataReader> ExecuteReader(string command, CommandType commandType = CommandType.Text, params IDataParameter[] @params)
        {
            var watch = StartWatch(command);

            var dbCommand = await CreateCommand(commandType, command, @params);

            try
            {
                var openTransaction = DbTransactionScope.Root;
                if (openTransaction == null)
                    return await dbCommand.ExecuteReaderAsync(CommandBehavior.CloseConnection);
                else
                {
                    var result = await dbCommand.ExecuteReaderAsync();
                    openTransaction.Register(result);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in running SQL Query.", ex).AddData("Command", command)
                    .AddData("Parameters", @params?.Select(p => p.ParameterName + "=" + p.Value).ToString(" | "))
                    .AddData("ConnectionString", dbCommand.Connection.ConnectionString);
            }
            finally
            {
                dbCommand.Parameters.Clear();
                if (watch != null) DataAccessProfiler.Complete(watch);
            }
        }

        /// <summary>
        /// Executes the specified command text against the database connection of the context and returns the single value of the type specified.
        /// </summary>
        public async Task<T> ExecuteScalar<T>(string commandText, CommandType commandType = CommandType.Text, params IDataParameter[] @params) =>
            (T)await ExecuteScalar(commandText, commandType, @params);

        /// <summary>
        /// Executes the specified command text against the database connection of the context and returns the single value.
        /// </summary>
        public async Task<object> ExecuteScalar(string command, CommandType commandType = CommandType.Text, params IDataParameter[] @params)
        {
            var watch = StartWatch(command);
            var dbCommand = await CreateCommand(commandType, command, @params);

            try
            {
                var result = await dbCommand.ExecuteScalarAsync();

                if (!command.ToLowerOrEmpty().StartsWith("select "))
                    DatabaseStateChangeCommand.Raise(command, commandType, @params);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in running Scalar SQL Command.", ex).AddData("Command", command)
                    .AddData("Parameters", @params?.Select(p => p.ParameterName + "=" + p.Value).ToString(" | "))
                    .AddData("ConnectionString", dbCommand.Connection.ConnectionString);
            }
            finally
            {
                dbCommand.Parameters.Clear();
                CloseConnection(dbCommand.Connection);
                if (watch != null) DataAccessProfiler.Complete(watch);
            }
        }

        /// <summary>
        /// Executes a database query and returns the result as a data set.
        /// </summary>        
        public async Task<DataTable> ExecuteQuery(string databaseQuery,
            CommandType commandType = CommandType.Text,
            params IDataParameter[] @params)
        {
            using (var reader = await ExecuteReader(databaseQuery, commandType, @params))
            {
                var table = new DataTable();
                table.Load(reader);
                return table;
            }
        }

        /// <summary>
        /// Executes the specified command text as nonquery.
        /// </summary>
        public async Task<int> ExecuteBulkNonQueries(CommandType commandType, List<KeyValuePair<string, IDataParameter[]>> commands)
        {
            var connection = await GetOrCreateConnection();
            var result = 0;

            try
            {
                foreach (var c in commands)
                {
                    var watch = StartWatch(c.Key);

                    DbCommand dbCommand = null;
                    try
                    {
                        dbCommand = await CreateCommand(commandType, c.Key, connection, c.Value);
                        result += await dbCommand.ExecuteNonQueryAsync();

                        DatabaseStateChangeCommand.Raise(c.Key, commandType, c.Value);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error in executing SQL command.", ex).AddData("Command", c.Key)
                            .AddData("Parameters", c.Value?.Select(p => p.ParameterName + "=" + p.Value).ToString(" | "));
                    }
                    finally
                    {
                        dbCommand?.Parameters.Clear();

                        if (watch != null) DataAccessProfiler.Complete(watch);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in running Non-Query SQL commands.", ex).AddData("ConnectionString", connection.ConnectionString);
            }
            finally
            {
                CloseConnection(connection);
            }
        }
    }
}