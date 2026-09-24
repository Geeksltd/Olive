# Olive.Entities.Data.DynamoDB

## Overview

The provided `Olive.Entities.Data.DynamoDB` library enables easy integration and usage of Amazon DynamoDB within Olive applications. It provides abstraction layers through convenient classes and methods to interact with DynamoDB, making CRUD operations efficient and straightforward.

---

## Table of Contents
- [Key Components](#key-components)
- [Dynamo](#dynamo-class)
- [DynamoDBDataProvider\<T\>](#dynamodbdataprovidert-class)
- [DynamoDBDataProviderFactory](#dynamodbdataproviderfactory-class)
- [DynamoRegistrationExtensions](#dynamoregistrationextensions-class)
- [Usage Examples](#usage-examples)

---

## Key Components

- **Dynamo**: Provides quick access to DynamoDB Context and Client for general purpose data operations.
- **DynamoDBDataProvider<T>**: Generic data provider for managing entities within DynamoDB.
- **DynamoDBDataProviderFactory**: Factory class for generating data providers for entity types.
- **DynamoRegistrationExtensions**: Registers DynamoDB services within .NET Core Dependency Injection system.

---

## Dynamo Class

### Static Properties:

| Property | Description |
|----------|-------------|
| `Client` | Provides the `IAmazonDynamoDB` client and accesses the underlying AWS DynamoDB service. |
| `Db`     | Provides DynamoDB Context (`IDynamoDBContext`) for performing database operations more conveniently.|

### Nested Classes:

1. `DbIndex<T>`: Access DynamoDB secondary indexes easily.
   - Methods:
     - `FirstOrDefault(object hashKey)`: Returns first entity or default for the provided hash key.
     - `All(object hashKey)`: Returns an array of all entities matching the hash key.

2. `DbTable<T>`: Access standard DynamoDB tables operations.
   - Methods:
     - `Get(object obj)`: Retrieves an entity by primary key.
     - `All(params ScanCondition[] conditions)`: Returns entities based on specified conditions.
     - `ForAll(...)`: Performs a parallel action on all entities matching scan conditions.

---

## DynamoDBDataProvider\<T\> Class

Implements Olive's `IDataProvider` interface to connect and manipulate entities in DynamoDB easily.

### Important Methods:

| Method | Description |
|--------|-------------|
| `Get(object objectID)` | Retrieves a single entity by its primary key. |
| `GetList(IDatabaseQuery query)` | Returns entities based on specified Olive queries (including hash key, index, or general conditions). |
| `Save(IEntity record)` | Saves changes (inserts new or updates existing entity). |
| `Delete(IEntity record)` | Deletes specified entity. |
| `BulkInsert(...)` | Inserts multiple entities in batch. |
| `BulkUpdate(...)` | Updates multiple entities in a batch (by deleting old and inserting updated versions). |

### Notes on Implementation:

- DynamoDB attributes like `[DynamoDBHashKey]` and `[DynamoDBGlobalSecondaryIndexHashKey]` are leveraged for efficient querying.
- Update operations are optimized by leveraging DynamoDB's capability to perform attribute updates.

---

## DynamoDBDataProviderFactory Class

This factory class generates and caches `DynamoDBDataProvider<T>` instances, improving performance by reusing existing providers.

### Methods:
- `GetProvider(Type type)`: Obtains or creates a cached provider for the specified entity type.
- `SupportsPolymorphism()`: Returns true, indicating DynamoDB supports entity polymorphism.

---

## DynamoRegistrationExtensions Class

This static class provides extension methods for easy registration of DynamoDB services using Microsoft's Dependency Injection pattern.

### Extension Method:
- `.AddDynamoDB(IServiceCollection services, IConfiguration config)`: Adds DynamoDB support to your .NET application startup configuration.

**Example usage (typically in Startup.cs):**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDynamoDB(Configuration);
}
```

---

## Usage Examples

### Example 1: Getting an entity from DynamoDB

```csharp
var provider = new DynamoDBDataProvider<User>();
var user = await provider.Get(userId);
```

### Example 2: Querying entities using hash key index

```csharp
var index = Dynamo.Index<User>("UserEmailIndex");
var matchingUser = await index.FirstOrDefault("test@example.com");
```

### Example 3: Saving an entity

```csharp
var provider = new DynamoDBDataProvider<User>();
var user = new User { Id = "123", Name = "John Doe" };
await provider.Save(user);
```

### Example 4: Bulk insert entities

```csharp
var provider = new DynamoDBDataProvider<User>();
var users = new IEntity[] { user1, user2, user3 };
await provider.BulkInsert(users, batchSize: 25);
```

### Example 5: Setting up DynamoDB in your .NET application

```csharp
services.AddDynamoDB(Configuration);
// other configurations...
```

---

## Special Notes:

- DynamoDB caching is implemented within `DynamoDBDataProviderFactory` to optimize performance.
- Changes in entities are tracked and finalized by methods like `OnEntityLoaded()`.
- Understand DynamoDB's data model (HashKey, SortKey, Indexes) to create efficient queries and schemas.

---

## Exception and Not Implemented Notices:

Certain methods (`Aggregate`, `ExecuteNonQuery`, etc.) directly inherited from Olive's `IDataProvider` are intentionally not implemented (`throw new NotImplementedException();`), as DynamoDB doesn't intrinsically support all relational database functionalities.

**Caution:** Use implemented methods compatible with DynamoDB operations.

---

## Dependencies

These utilities require AWS DynamoDB SDK (`Amazon.DynamoDBv2`) and related packages (`Amazon.DynamoDBv2.DataModel`) to be installed and configured.

---

## Configuration

Ensure configuration keys in your `appsettings.json` (or equivalent) contains AWS credentials and endpoints if required:

```json
{
  "AWS": {
    "Profile": "default",
    "Region": "us-west-2"
  }
}
```

Alternatively, when using LocalStack for local AWS testing, ensure the correct LocalStack endpoint is configured. 