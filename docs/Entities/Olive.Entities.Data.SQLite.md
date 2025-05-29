# Olive.Entities.Data.SQLite

## Overview

The Olive framework provides a dedicated SQLite data adapter enabling applications to efficiently store and query data using an embedded SQLite database. This adapter includes specific utilities such as generating SQL commands, handling database criteria expressions tailored for SQLite syntax, managing database lifecycle, and providing seamless integration with the Olive data access layer.

SQLite can be a great choice particularly for mobile, desktop applications, testing, or lightweight embedded database usage scenarios.

---

## Table of Contents

- [Key Components](#key-components)
- [SqliteCommandGenerator](#sqlitecommandgenerator)
- [SqliteCriterionGenerator](#sqlitecriteriongenerator)
- [SqLiteManager](#sqlitemanager)
- [Examples](#examples)
- [Configuration](#configuration)
- [Dependencies](#dependencies)

---

## Key Components

The SQLite adapter contains the following core components:

- **SqliteCommandGenerator:** Generates SQLite-compatible SQL commands.
- **SqliteCriterionGenerator:** Generates SQLite-specific query criteria expressions.
- **SqLiteManager:** Manages SQLite database operations and schema management.

---

## SqliteCommandGenerator

This class generates SQL command strings specifically tailored for the SQLite DBMS, fully supporting pagination and accompanying query logic.

### Main Methods:

- **`GenerateSelectCommand(IDatabaseQuery, string tables, string fields)`**  
  Constructs SQLite-compatible SELECT statements using query filters, sorting, and pagination features.

- **`GeneratePagination(IDatabaseQuery query)`**  
  Implements SQLite LIMIT/OFFSET syntax to paginate query results efficiently.

- **`SafeId(string id)`**  
  Safely escapes an SQLite identifier (table or column names) using backticks.

- **`UnescapeId(string id)`**  
  Removes escaping from SQLite identifiers.

**Example Usage:**  
```csharp
var generator = new SqliteCommandGenerator();
var query = new DatabaseQuery(typeof(Product))
{
    PageStartIndex = 5,
    TakeTop = 10
};

string selectCommand = generator.GenerateSelectCommand(query, "`Products`", "*");
// Result: SELECT * FROM `Products` LIMIT 10 OFFSET 5
```

---

## SqliteCriterionGenerator

Generates SQLite SQL fragments for entity criteria expressions such as Where clauses. It ensures identifiers follow SQLite-compatible naming and escaping.

- **Identifier Escaping:** Uses backticks (` ` `) for safe SQLite identifier escaping.

---

## SqLiteManager

Handles SQLite database operations such as schema execution, database deletion, and command operation execution.

### Main Methods:
- **`Delete(string databaseName)`**  
  Drops all tables from the SQLite database as SQLite doesn't support dropping databases directly.

- **`Execute(string sql, string database = null)`**  
  Executes arbitrary SQL commands against the SQLite database.

- **`ClearConnectionPool()`**  
  No operation (noop), as connection pooling is not applicable in SQLite.

- **`Exists(string database, string filePath)`**  
  Currently returns always `false` as SQLite's database existence logic is file-based externally managed.

**Important**: The `Delete` method drops all tables separately using SQLite system tables as dropping databases entirely is not supported by SQLite.

---

## Examples

### Basic SQLite Database Creation and Initialization Example:
```csharp
var manager = new SqLiteManager(providerConfig);
string createTableScript = "CREATE TABLE `Users` (`ID` TEXT PRIMARY KEY, `Name` TEXT)";

manager.Execute(createTableScript);
```

### Generating a paginated SELECT command for SQLite:

```csharp
var query = new DatabaseQuery(typeof(User)) { PageStartIndex = 0, TakeTop = 5 };
var commandGenerator = new SqliteCommandGenerator();

string command = commandGenerator.GenerateSelectCommand(query, "`Users`", "*");
// Generated SQL: SELECT * FROM `Users` LIMIT 5
```

---

## Configuration

In your `appsettings.json` or configuration source, make sure to define your SQLite connection string:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=appdata.db;"
  }
}
``` 

---

## Dependencies

Ensure the official SQLite package is referenced in your Olive application.

- SQLite provider via official Microsoft ADO.NET SQLite package (NuGet):

```powershell
Install-Package Microsoft.Data.Sqlite
```

- Olive framework (`Olive.Entities`) NuGet package is also required:

```powershell
Install-Package Olive.Entities
```

---

## Important Notes

- By SQLite's nature, some standard SQL operations (dropping databases, certain ALTER operations) aren't supported or are limited. This adapter includes sensible workarounds where applicable.
- SQLite doesn't have connection pooling, so related operations in Olive's data layer are safely ignored.
- Proper identifier escaping is provided throughout Olive's SQLite adapter classes to avoid common issues with special keywords or reserved identifiers.

---

## Exception Handling

The adapter includes meaningful, Olive-specific exception messages clarifying SQL execution issues and data access errors, aiding rapid troubleshooting.  

**Example exception thrown by the manager**:

```
Exception: "Could not drop database 'MyDatabase'" (Inner exception details)
```

---

## Notes

- SQLite lacks support for some database features such as multi-database setups, extensive security, and certain schema features. It is therefore recommended primarily for development, local, or lightweight applications.