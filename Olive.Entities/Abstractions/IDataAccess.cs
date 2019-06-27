namespace Olive.Entities
{
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides direct data access to the underlying data source.
    /// </summary>
    public interface IDataAccess
    {
        Task<IDbConnection> GetOrCreateConnection();

        Task<IDbConnection> CreateConnection();

        Task<IDataReader> ExecuteReader(string command, CommandType commandType = CommandType.Text, params IDataParameter[] @params);

        Task<DataTable> ExecuteQuery(string databaseQuery, CommandType commandType = CommandType.Text, params IDataParameter[] @params);

        Task<int> ExecuteNonQuery(string command, CommandType commandType = CommandType.Text, params IDataParameter[] @params);

        Task<object> ExecuteScalar(string command, CommandType commandType = CommandType.Text, params IDataParameter[] @params);

        Task<T> ExecuteScalar<T>(string command, CommandType commandType = CommandType.Text, params IDataParameter[] @params);

        Task<int> ExecuteBulkNonQueries(CommandType commandType, List<KeyValuePair<string, IDataParameter[]>> commands);

        IDataParameter CreateParameter(string name, object value);

        IDataParameter CreateParameter(string name, object value, DbType? dbType);

        ISqlCommandGenerator GetSqlCommandGenerator();
    }
}
