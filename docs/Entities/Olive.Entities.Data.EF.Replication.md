# Olive.Entities.Data.EF.Replication

## Overview

The Olive Entities EF Replication system simplifies database replication and synchronization across multiple data stores and services. It leverages Entity Framework (EF) and the Olive framework to provide flexible mechanisms to replicate, sync, and expose data via event-driven replication queues, enabling effective management of data movement across different system components.

---

## Table of Contents

- [Key Components](#key-components)
- [ExposedType Classes](#exposedtype-classes)
  - [ExposedType<TDomain>](#exposedtypetdomain-class)
  - [HardDeletableExposedType](#harddeletableexposedtype-class)
  - [NakedExposedType & HardDeletableNakedExposedType](#nakedexposedtype--harddeletablenakedexposedtype-classes)
- [ExposedField Classes](#exposedfield-classes)
- [Replication Attributes](#replication-attributes)
- [Source and Destination Endpoints](#source-and-destination-endpoints)
- [Replication Messages](#replication-messages)
- [Examples](#examples)
- [Configuration](#configuration)

---

## Key Components

- **Exposed Types**: Define and expose domain models and their fields for replication.
- **Endpoints**: SourceEndpoint and DestinationEndpoint manage sending and receiving replicated data.
- **Replication Messages**: Structured messages (`ReplicateDataMessage`) for data synchronization.
- **Attributes**: Customize behavior through declarative attributes like `[ExportData]` and `[EventBus]`.

---

## ExposedType Classes

### ExposedType\<TDomain> Class

Abstract base class to declare exposed entities and fields for replication.

#### Important Members:

- `Expose(...)`: Define properties to expose.
- `AddDependency(...)`: Declare dependencies for associated entities.
- `AddChildDependency(...)`: Define reverse (one-to-many) entity relationships.
- `ExposeEverything()`: Automatically expose all suitable properties.
- `Filter()` and `FilterAsync()`: Customize which records get replicated.

### Example:

```csharp
public class UserExposedType : ExposedType<User>
{
    public override void Define()
    {
        Expose(u => u.FirstName);
        Expose(u => u.LastName);
        Expose(u => u.Email);
        AddDependency(u => u.Department);
    }
}
```

---

### HardDeletableExposedType Class

Derives from `ExposedType`. Overrides the soft-deletion behavior to implement hard deletes instead.

- `IsSoftDeleteEnabled`: Returns `false`, indicating the records are physically deleted on replication targets.

---

### NakedExposedType & HardDeletableNakedExposedType Classes

Automatically expose all fields of the entity without explicitly defining each property.

```csharp
public class NakedUserExposedType : NakedExposedType<User> { } // Exposes all fields implicitly.
```

---

## ExposedField Classes

### ExposedPropertyInfo and CustomExposedField

These classes manage exposure of domain properties and derived or custom computed properties for replication.

- `ExposedPropertyInfo`: Automatically manages serialization of regular domain properties.
- `CustomExposedField`: For custom, computed, or derived properties.

### Example:

```csharp
Expose("FullName", user => user.FirstName + " " + user.LastName);
```

---

## Replication Attributes

### ExportDataAttribute

Marks data types (exposed types) to be included in endpoint replication.

```csharp
[ExportData(typeof(UserExposedType))]
public class UserEndpoint : SourceEndpoint { }
```

### EventBusAttribute

Defines the replication queue endpoint URLs.

```csharp
[EventBus(environment: "Production", url: "https://sqs.myqueue.aws.amazon.com")]
public class UserEndpoint : SourceEndpoint { }
```

---

## Source and Destination Endpoints

### SourceEndpoint

Publishes data changes from source domain models onto replication queues.

#### Key Features:

- `Publish()`: Starts publishing handlers.
- `UploadAll()`: Full upload (e.g., for replication initialization or refreshes).
- Automatically handles saving events and publishes messages.

### DestinationEndpoint

Consumes and applies replication data messages to local databases.

#### Key Features:

- `Subscribe()`: Starts listening to queue messages to replicate incoming changes.
- Event-driven architecture provides replication events (`Saving`, `Saved`, `Deleting`, `Deleted`) to hook custom logic.

### EndpointSubscriber

Internal class handling deserialization, importing, and updating local database records based on messages.

---

## Replication Messages

Structured communication objects encapsulating replicated data:

- `ReplicateDataMessage`: Contains serialized data, metadata, and operation type (delete or update).
- `RefreshMessage`: Signals complete data refresh per entity type.

---

## Examples

### Defining a Simple Exposed Type

```csharp
class ProductExposedType : ExposedType<Product>
{
    public override void Define()
    {
        Expose(p => p.Name);
        Expose(p => p.Price);
    }
}
```

### Setting Up a Source Endpoint to Publish Data

```csharp
[ExportData(typeof(ProductExposedType))]
public class ProductsSourceEndpoint : SourceEndpoint
{
    public void StartReplication()
    {
        Publish();
    }
}
```

### Destination Endpoint Subscription and Handling

```csharp
public class ProductsDestEndpoint : DestinationEndpoint
{
    public ProductsDestEndpoint(DbContext dbContext)
        : base(typeof(Product).Assembly, dbContext)
    {
        Register(typeof(Product).FullName);
    }

    public async Task StartListening()
    {
        Saved += async e => Console.WriteLine($"Saved Product {e.Instance.GetId()}");
        await Subscribe();
    }
}
```

---

## Configuration

### EventBus URL configuration example (`appsettings.json`):

```json
{
  "EventBus": {
    "QueueUrls": {
      "ProductsSourceEndpoint": "https://sqs.myqueue.aws.amazon.com/123456789/products"
    }
  }
}
```

---

## Notes

- Ensure proper EF data annotations (`[Table]`, `[Column]`, `[DynamoDBHashKey]`) are used for accurate mapping.
- Carefully review soft/hard delete behavior as it impacts data integrity.
- Monitor replication events to manage consistency and handle exceptions effectively.