# Raw Data Access
Olive facilitates raw data access in your business applications by eliminating the typical ADO.NET mess in relation to creating connections, commands, handling transactions, etc. It provides you with a simple API to reliably invoke raw database commands against any kind of database system.

## appSettings.json and Connection Strings
To access a database, you will need to provide a connection string. By default, the data access framework components in Olive expect the connection strings to be specified in the application config files such as `appSettings.json` or its environment-specific alternatives such as `appSettings.Development.json` or `appSettings.Production.json`.

By convention, each database should be specified with a key under the root node of `ConnectionStrings`.
The following example, introduces two databases, each with a connection string key: one called **Default** and one called **ProductCatalogue**.  

```json
{
    "ConnectionStrings": {
        "Default": "Database=MyDatabase; Server=.\\SQLExpress; Integrated Security=SSPI; MultipleActiveResultSets=True;",
        "ProductCatalogue": "Database=Products; Server=SomeServer; user=sa; password=1234566; MultipleActiveResultSets=True;"
    },
    ...
}
```
Most applications use a single database. Because of that, in Olive, by default, when you don't specify the connection string, the config value under the key `"ConnectionStrings:Default"` will be used. 


## IDataAccess interface
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

This inteface is used in the Olive data access framework for gaining access to low level data operations. You can provide your own custom implemation for any database technology. Olive already provides implementations for this interface for common database technologies including **SQL Server**, **MySQL**, **PostgreSql**, **SqLite**.

## DataAccess class
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
> Avoid running commands against the database directly, except for exceptional scenarios. In most cases, you should use the ORM Api due to many benefits that it provides, including strong typing and better maintainability.

### Startup.cs
In the application startup class, you should specify the default data access engine for your application. For example, to use SqlServer data access, add the following line to your `ConfigureServices()` method.

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    services.AddDataAccess(x => x.SqlServer());
    ...
}
```

## DatabaseContext class
Most applications deal with a single database. Because of that, for maximum clarity and simplicity, Olive data access APIs do not require you to explicitly specify the connection string or the data access middleware technology.

There are cases however, when your application deals with more than one database. To allow you to explicitly specify the target database that you intend to use, Olive provides the `DatabaseContext` class. For example:

```csharp
using (new DatabaseContext(Config.Get("ConnectionStrings:ProductCatalogue")))
{
    // Any data access operation written here will be executed against that database.
}
```
Once the execution exists the `using` block, all data operations will be diverted to the primary database with the default connection string again.

## ACID Transactions
The Olive data access framework provides a declarative transaction style for atomic (all success or all rollback) operations that involve multiple data operations.

```csharp
using (var scope = Database.CreateTransactionScope())
{
     // ... operation A
     // ... operation B
     scope.Complete();
}
```
If any of the operations fails, the whole transaction will be rolled back.

`Complete()` commits. Anything run after it in the same block (sending a notification, reading the saved record back) runs on its own, outside the committed transaction: a write there is kept even if a later one fails, and a scope opened there starts a transaction of its own. A handler given to `OnTransactionCompleted` after the commit runs straight away.

### A write allowed to fail inside a transaction
Sometimes one write in a transaction may fail without the rest being lost, such as a counter that is nice to have. Catching its exception is not enough everywhere: on PostgreSQL one failed statement aborts the whole transaction, and every command after it fails too. `BestEffort` puts the write behind a savepoint and rolls back to it on failure, which leaves the transaction usable on any database:

```csharp
using (var scope = Database.CreateTransactionScope())
{
    var access = Database.GetAccess();

    if (!await access.BestEffort(() => Database.Update(counter, c => c.Count++)))
        Log.For(this).Warning("The counter was not updated.");

    // ... operations that must commit either way
    scope.Complete();
}
```
It returns whether the write succeeded and swallows its exception. Outside a transaction scope there is nothing to protect, so it only catches. Each provider writes its own savepoint SQL (`SqlCommandGenerator.GenerateSavepoint`, `GenerateRollbackToSavepoint`, `GenerateReleaseSavepoint`; SQL Server's `SAVE TRANSACTION`, the standard `SAVEPOINT` elsewhere).

## Recognising a command timeout
Each driver reports a command that ran out of time its own way (SQL Server: `SqlException` number -2; PostgreSQL: a `TimeoutException` inside an `NpgsqlException`, or the server's `query_canceled`), and Olive may wrap it. `access.IsTimeout(exception)` checks the exception and everything it wraps against the configured provider, so code giving up on a slow query names no database:

```csharp
try { return await query(); }
catch (Exception ex) when (Database.GetAccess().IsTimeout(ex)) { return default; }
```
A provider decides in `SqlCommandGenerator.IsTimeout`; one that does not override it recognises a plain `TimeoutException`.
