# Olive.Entities.Data.SqlServer

## Overview

The Olive Entities SQL Server adapter enables applications built on the Olive Framework to integrate efficiently with Microsoft SQL Server databases. It facilitates the generation and execution of optimized SQL queries specific to SQL Server's syntax and features. The adapter comprises command generators, criterion generators, database lifecycle management utilities, and simple integration extensions with Olive's Data Access infrastructure.

---

## Table of Contents

- [Key Components](#key-components)
- [SqlServerSqlCommandGenerator](#sqlserversqlcommandgenerator)
- [SqlCriterionGenerator](#sqlcriteriongenerator)
- [SqlServerManager](#sqlservermanager) 
- [Example Usages](#example-usages)
- [Configuration](#configuration)
- [Dependencies](#dependencies)
- [Exception Handling and Logging](#exception-handling-and-logging)

---

## Key Components

- **SqlServerSqlCommandGenerator**: Generates SQL Server-compatible SQL queries.
- **SqlCriterionGenerator**: Creates query conditions using appropriate SQL Server escaping.
- **SqlServerManager**: Handles SQL Server-specific database operations, including deletion and detachment.

---

## SqlServerSqlCommandGenerator

Generates SQL commands tailored explicitly for SQL Server, with special consideration for pagination and sorting:

### Primary Methods:

- **`GenerateSelectCommand()`**: Constructs SELECT statements, optionally applying TOP, ORDER BY, and pagination clauses.
- **`GeneratePagination()`**: Uses SQL Server's `OFFSET...FETCH NEXT` pagination syntax.
- **`SafeId(string id)`**: Safely wraps SQL identifiers (columns, tables) in square brackets `[...]`.
- **`UnescapeId(string id)`**: Removes square brackets from identifiers.

#### Example:

```csharp
var commandGenerator = new SqlServerSqlCommandGenerator();

var query = new DatabaseQuery(typeof(Customer))
{
    PageStartIndex = 20,
    PageSize = 10,
    OrderBy("LastName")
};

string sql = commandGenerator.GenerateSelectCommand(query, "[Customers]", "[ID], [FirstName], [LastName]");

// Generated SQL:
SELECT [ID], [FirstName], [LastName] FROM [Customers]
ORDER BY [LastName] OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY
```

---

## SqlCriterionGenerator

Designed specifically for generating SQL Server query criteria (expressions used in WHERE clauses), using identifier escaping with square brackets:

- **Identifier Escaping**: `[Identifier]`.
- **Identifier Unescaping**: Identifier trimmed of `[ ]`.

---

## SqlServerManager

Performs database management tasks specifically targeted at SQL Server, such as deleting or detaching databases, managing connections, and executing scripts:

### Common Methods:

- **`Delete(string databaseName)`**: Deletes an entire database safely by performing necessary user connection management.
- **`Detach(string databaseName)`**: Detaches the database by first disabling connections to ensure safe detachment.
- **`Execute(string sql, string database = null)`**: Executes arbitrary SQL batches. Automatically splits provided scripts on "GO" batch terminators.
- **`Exists(string database, string filePath)`**: Checks if the specified database exists in SQL Server.

#### Example Database Deletion:

```csharp
var manager = new SqlServerManager();
manager.Delete("TestDatabase");

// Generates and executes:
// ALTER DATABASE [TestDatabase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
// ALTER DATABASE [TestDatabase] SET MULTI_USER;
// DROP DATABASE [TestDatabase];
``` 

---

## Example Usages

### Registering SQL Server Data Access in Olive Application

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        DataAccess.Configure(options => options.SqlServer());
    }
}
```

### Executing Scripts with SQL Server Manager

```csharp
var manager = new SqlServerManager();

string sqlScript = @"CREATE TABLE [Users](
                        [ID] UNIQUEIDENTIFIER PRIMARY KEY,
                        [Name] NVARCHAR(100)
                    );GO
                    INSERT INTO [Users] VALUES (NEWID(), 'Alice');";

manager.Execute(sqlScript, "MyDatabase");
```

### Generating Paginated Select Command

```csharp
var query = new DatabaseQuery(typeof(User)) { PageStartIndex = 0, PageSize = 5 };
query.OrderBy("Name");

var sqlGenerator = new SqlServerSqlCommandGenerator();
string command = sqlGenerator.GenerateSelectCommand(query, "[Users]", "[ID], [Name]");

// Generated SQL:
// SELECT [ID], [Name] FROM [Users] ORDER BY [Name] OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY
```

---

## Configuration

To enable SQL Server in your Olive application, define a SQL Server connection string in your `appsettings.json` file or other configuration source:

```json
{
  "ConnectionStrings": {
    "Default": "Server=myServer;Database=myDb;User Id=myUser;Password=myPassword;"
  }
}
``` 
---

## Dependencies

Make sure the following NuGet packages are installed and referenced in your Olive Application:

- Official Microsoft SQL Client Provider for .NET:
```powershell
Install-Package Microsoft.Data.SqlClient
```

- Core Olive Libraries:
```powershell
Install-Package Olive.Entities
```

---

## Exception Handling and Logging

The adapter provides detailed exception handling with clear, actionable error messages:

Example Exception Thrown:
```
Exception: "Could not drop database 'TestDB'" (Inner exceptions detailing the cause)
```

Logging:

- Comprehensive logging of database operations and exceptional scenarios uses Olive's built-in logging mechanisms.
- SQL execution failures log the executed command for ease of troubleshooting.

---

## Important Notes

- Ensure that proper permissions exist for database operations executed through Olive's SQL Server adapter.
- Use square brackets `[identifier]` when dealing with identifiers that match SQL Server reserved keywords or contain spaces.
- By default, the generated SQL uses modern SQL Server pagination syntax (OFFSET...FETCH NEXT), requiring SQL Server 2012 or newer.