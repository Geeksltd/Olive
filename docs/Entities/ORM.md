# Olive ORM (Object Relational Mapping)

Olive entities are basic C# objects. You can therefore use them with any ORM technology, such as *Entity Framework*. Though, they won't invoke the lifecycle events, and you will need to be careful in how you write your business logic related to object lifecycle.

Olive provides an ORM framework that is specifically designed for easier use, higher flexibility and better performance than Entity Framework or almost any other ORM technology. For example, in executing queries, it's on average twice as performant as Entity Framework.

## Core Concepts
The Olive ORM framework provides the following essential components.

### IDataAccess
This abstraction provides direct data access to the underlying data source. An implementation of `IDataAccess` implements all of its essential database operations including:
```
Task<IDbConnection> CreateConnection(string connectionString);
Task<IDataReader> ExecuteReader(string command, CommandType commandType, params IDataParameter[] @params);
Task<DataTable> ExecuteQuery(string databaseQuery, CommandType commandType, params IDataParameter[] @params);
Task<int> ExecuteNonQuery(string command, CommandType commandType, params IDataParameter[] @params);
Task<object> ExecuteScalar(string command, CommandType commandType, params IDataParameter[] @params);
Task<T> ExecuteScalar<T>(string command, CommandType commandType, params IDataParameter[] @params);
Task<int> ExecuteBulkNonQueries(CommandType commandType, List<KeyValuePair<string, IDataParameter[]>> commands);
IDataParameter CreateParameter(string name, object value, DbType? dbType);
```

This inteface is used in the Olive data access framework for gaining access to low level data operations. You can provide your own custom implemation for any database technology. Olive already provides implementations for this interface for common database technologies including: 

- SQL Server
- MySQL
- PostgreSql
- SqLite

#### DataAccess
This abstract class provides a basic implementation of the `IDataAccess` for all Ado.NET providers. It is used as the base class of the technology-specific providers such as the built-in `SqlServerDataAccess`. 

In addition, this class provides a number of static methods to enable you to execute raw database commands, without worrying about connection strings, connections, error handling, etc.

- `DataAccess.GetCurrentConnectionString()` returns the default connection string from the application config file.
- `DataAccess.GetDataAccess(providerType)` returns an `IDataAccess` instance for the specified provider type. This is useful when you have more than one database technology in the same application. For example to gain a `IDataAccess` for executing commands against SQL Server you can invoke `DataAccess.GetDataAccess("System.Data.SqlClient")`

### IDataProvider

### IDatabase

### IDatabaseQuery<T>
  


