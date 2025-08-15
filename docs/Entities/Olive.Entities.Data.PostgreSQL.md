# Olive.Entities.Data.PostgreSQL

## Overview
Olive's PostgreSQL Data Adapter provides seamless integration and compatibility of PostgreSQL databases within the Olive Entities framework. It includes specialized implementations for generating PostgreSQL-specific SQL commands and criteria handling, enabling efficient data access through the Olive data access layer.

---

## Table of Contents
- [Key Components](#key-components)
- [PostgreSqlCommandGenerator Class](#postgresqlcommandgenerator-class)
- [PostgreSqlCriterionGenerator Class](#postgresqlcriteriongenerator-class)
- [PostgreSqlDataAccessExtensions Class](#postgresqldataaccessextensions-class)
- [Usage Examples](#usage-examples)
- [Configuration](#configuration)
- [Dependencies](#dependencies)

---

## Key Components
- **PostgreSqlCommandGenerator**: Generates PostgreSQL-compatible SQL commands.
- **PostgreSqlCriterionGenerator**: Handles query conditions specifically tailored for PostgreSQL.
- **PostgreSqlDataAccessExtensions**: Provides simplified integration within Olive's DataAccess configuration.

---

## PostgreSqlCommandGenerator Class

Generates PostgreSQL-specific SQL command strings for querying entities, providing efficient queries with correct pagination and syntax.

### Methods:

- **GenerateSelectCommand(IDatabaseQuery iquery, string tables, string fields)**  
  Creates a SELECT statement including fields, tables, criteria, sorting, and pagination.

- **GeneratePagination(IDatabaseQuery query)**  
  Appends LIMIT and OFFSET clauses specific to PostgreSQL.

- **SafeId(string id)**  
  Properly escapes identifiers using double-quotes (`"`), following PostgreSQL conventions.

- **UnescapeId(string id)**  
  Removes escaping quotes from identifiers.

### Example:
```csharp
var generator = new PostgreSqlCommandGenerator();

var query = new DatabaseQuery(typeof(User))
{
    PageStartIndex = 20,
    TakeTop = 10
};

string selectQuery = generator.GenerateSelectCommand(query, "Users", "ID, Name, Email");
```

---

## PostgreSqlCriterionGenerator Class

Generates PostgreSQL-specific criteria SQL fragments for querying databases.

- Internally used by the Olive Framework.
- Uses double-quotes (`"`) for identifier quoting.

Implementation example (for reference, normally internal):

```csharp
protected override string ToSafeId(string id) => $"\"{id}\"";
protected override string UnescapeId(string id) => id.Trim('\"');
```

---

## PostgreSqlDataAccessExtensions Class

Enables easy integration of PostgreSQL provider and command generation in DataAccess configuration via extension methods.

### Methods:
- **PostgreSql()**: Configures Olive's data access layer to use PostgreSQL. 
- 
---

## Usage Examples 

### Generating a PostgreSQL SELECT Command
```csharp
var commandGenerator = new PostgreSqlCommandGenerator();

var query = new DatabaseQuery(typeof(Product))
              {
                  TakeTop = 5,
                  PageStartIndex = 10
              };

string sqlCommand = commandGenerator.GenerateSelectCommand(query, "Products", "*");
// Resulting SQL:
// SELECT * FROM Products LIMIT 5 OFFSET 10
```

---

## Configuration

Setup a PostgreSQL connection string in your application's settings (e.g. `appsettings.json`):

```json
{
  "ConnectionStrings": {
    "Default": "Host=myserver;Database=mydb;Username=mylogin;Password=mypass"
  }
}
``` 

---

## Dependencies

This adapter requires adding the Npgsql library, an open-source .NET data provider for PostgreSQL:

```bash
Install-Package Npgsql
```

---

## Important Notes

- PostgreSQL identifiers use double-quotes (`"`) to preserve case sensitivity.
- Ensure proper escaping to avoid syntax errors or injection vulnerabilities.
- Olive automatically handles correct escaping through methods provided in command and criteria generators.