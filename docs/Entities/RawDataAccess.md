# Raw Data Access


### IDataAccess
This abstraction provides direct data access to the underlying data source. An implementation of `IDataAccess` implements all of its essential database operations including:
```csharp
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
- `DataAccess.GetDataAccess()` returns an `IDataAccess` instance for the default provider type. This is useful when you have a single database technology in your application.

For example, if you want to execute a SQL command directly in your business logic, you can do:
```csharp
async Task SomeMethod()
{
    await DataAccess.GetDataAccess().ExecuteNonQuery("UPDATE Customers SET IsArchived = 1");
}
```
Though, please note that you should avoid running commands against the database directly except for exceptional scenarios.
In most cases, you should use the ORM Api due to many benefits that it provides, including strong typing and better maintainability.

## AppSettings.json
...

Also, you will need to register the default data provider in the `Startup.cs` class, under the `ConfigureServices()` method.
...

## DatabaseContext
Most applications deal with a single database. Because of that, for maximum clarity and simplicity, Olive data access APIs do not require you to explicitly specify the connection string or the data access middleware technology.

There are cases however, when your application deals with more than one database. To allow you to explicitly specify the target database that you intend to use, Olive provides the `DatabaseContext` class. For example:

```csharp
using (new DatabaseContext(Config.Get("Database:MyDatabase:ConnectionString")))
{
    // Any data access operation written here will be executed against that database.
}
```
Once the execution exists the `using` block, all data operations will be diverted to the primary database again.

