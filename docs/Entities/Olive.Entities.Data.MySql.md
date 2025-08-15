# Olive.Entities.Data.MySql

## Overview
The Olive Framework's MySQL Adaopter provides seamless integration of MySQL databases with Olive's data access infrastructure. It offers implementations for connection management, SQL generation, and database commands optimized specifically for MySQL.

---

## Table of Contents
- [Key Components](#key-components)
- [MySqlManager Class](#mysqlmanager-class)
- [MySqlCommandGenerator Class](#mysqlcommandgenerator-class)
- [MySqlConnectionStringProvider Class](#mysqlconnectionstringprovider-class)
- [MySqlDataAccessExtensions](#mysqldataaccessextensions-class)
- [MySqlCriterionGenerator Class](#mysqlcriteriongenerator-class)
- [ParameterFactory](#parameterfactory-class)
- [Examples](#examples)
- [Configuration](#configuration)

---

## Key Components
- **MySqlManager**: Manages database operations including executing scripts, clearing pools, and checking database existence.
- **MySqlCommandGenerator**: Generates MySQL-specific SQL commands for commonly used queries.
- **MySqlConnectionStringProvider**: Provides specialized MySQL connection strings.
- **MySqlDataAccessExtensions**: Extension methods for simplified MySQL integration with Olive DataAccess.
- **MySqlCriterionGenerator**: Generates criteria expressions tailored to MySQL.
- **ParameterFactory**: Generates MySQL-compatible parameters for database commands.

---

## MySqlManager Class

Manages common database operations for MySQL, such as executing commands, checking if databases exist, and clearing connection pools.

#### Methods:

- **ClearConnectionPool()**
  Clears all MySQL connection pools.

- **Delete(string database)**
  Drops the specified database.
  
- **Execute(string script, string database = null)**
  Executes provided SQL script.

- **Exists(string database, string filePath)**
  Checks if the specified database exists.

**Example Usage:**
```csharp
var manager = new MySqlManager();

// Execute script
manager.Execute("CREATE TABLE Users (ID INT PRIMARY KEY);", "MyDatabase");

// Check database Existence
bool exists = manager.Exists("MyDatabase", "/var/lib/mysql/MyDatabase");
```

---

## MySqlCommandGenerator Class

Generates SQL commands tailored specifically for MySQL databases.

#### Methods:

- **GenerateSelectCommand(...)**
  Generates SELECT statements including pagination and sorting.

- **GeneratePagination(...)**
  Adds pagination logic (`LIMIT`) to queries.

- **SafeId(string id)**
  Safely escapes identifiers with backticks.

**Example Usage:**
```csharp
var generator = new MySqlCommandGenerator();
string sql = generator.GenerateSelectCommand(query, "Users", "*");
```

---

## MySqlConnectionStringProvider Class

Customizes connection strings specifically for MySQL databases, including additional MySQL-specific settings.

#### Methods:

- **GetConnectionString(string key = "Default")**
  Returns a connection string optimized for MySQL.

**Example Usage:**
```csharp
var provider = new MySqlConnectionStringProvider();
string connString = provider.GetConnectionString();
```

---

## MySqlDataAccessExtensions Class

Extension methods allowing simplified MySQL integration with Olive's DataAccessOptions.

#### Methods:

- **MySql()**
  Adds MySQL-specific implementations of command generators and parameter factories.

**Example Usage:**
```csharp
dataAccessOptions.MySql();
```

---

## MySqlCriterionGenerator Class

Generates SQL snippets for database criteria expressions that match MySQL syntax.

**Example Usage (internal use):**
This class is used internally by Olive's data query mechanism.

---

## ParameterFactory Class

Generates parameters compatible with MySQL commands. Used internally by Olive's data access layer.

---

## Examples

### Configuring DataAccess to Use MySQL

```csharp
DataAccess.Configure(options => options.MySql());
```

### Executing SQL Script Using MySqlManager

```csharp
// Initialize MySqlManager
var manager = new MySqlManager();

// Execute commands on database
manager.Execute("DROP TABLE IF EXISTS Users; CREATE TABLE Users(ID INT PRIMARY KEY AUTO_INCREMENT);", "MyDatabase");
```

### Generating Select Query Using MySqlCommandGenerator

```csharp
var generator = new MySqlCommandGenerator();

var query = new DatabaseQuery(typeof(User))
{
    PageStartIndex = 0,
    TakeTop = 10
};

string sqlCommand = generator.GenerateSelectCommand(query, "Users", "ID, UserName");
```

---

## Configuration

To configure your Olive application for MySQL, ensure your app configuration (`appsettings.json`, `web.config`, etc.) includes a valid MySQL connection string:

```json
{
  "ConnectionStrings": {
    "Default": "Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;"
  }
}
```

Furthermore, enable MySQL extensions as follows:

```csharp
DataAccess.Configure(o => o.MySql());
```

---

## Important Notes

- Ensure the MySql.Data library is available in your project.
- Use backticks (`) to safely escape column and table names, as provided by the `SafeId` method in `MySqlCommandGenerator`.
- The provided classes handle internal tasks of Olive Framework and typically do not require manual invocation in everyday use.

---

## Dependencies

Make sure you're referencing MySql.Data library to use Olive Entities MySQL Adapter:

- **NuGet Package Installation:**
```bash
Install-Package MySql.Data
```

---

## Exception Handling
The methods are wrapped to throw meaningful exceptions in case of failures (e.g., cannot establish database connections or invalid SQL scripts). Use try-catch blocks in your application as needed.