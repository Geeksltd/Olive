namespace Olive.Entities
{
    /// <summary>
    /// Provides direct data access to the underlying data source.
    /// </summary>
    public interface IDataAccess
    {
        Task<IDbConnection> GetOrCreateConnection();

        Task<IDbConnection> CreateConnection(string connectionString = null);

        Task<IDataReader> ExecuteReader(string command, CommandType commandType = CommandType.Text, params IDataParameter[] @params);

        Task<DataTable> ExecuteQuery(string databaseQuery, CommandType commandType = CommandType.Text, params IDataParameter[] @params);

        Task<int> ExecuteNonQuery(string command, CommandType commandType = CommandType.Text, params IDataParameter[] @params);

        Task<object> ExecuteScalar(string command, CommandType commandType = CommandType.Text, params IDataParameter[] @params);

        Task<T> ExecuteScalar<T>(string command, CommandType commandType = CommandType.Text, params IDataParameter[] @params);

        Task<int> ExecuteBulkNonQueries(CommandType commandType, List<KeyValuePair<string, IDataParameter[]>> commands);
    }
}
