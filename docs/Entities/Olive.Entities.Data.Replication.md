# Olive.Entities.Data.Replication

## Overview
Olive Entities Replication provides a structured and efficient mechanism for replicating and synchronizing entity data across multiple database instances or services. It leverages an event-driven architecture to enable seamless data replication, synchronization, and consistent maintenance of entities across different endpoints.

---

## Table of Contents
- [Key Components](#key-components)
- [ExposedType Classes](#exposedtype-classes)
  - [ExposedType\<TDomain\>](#exposedtype-tdomain)
  - [NakedExposedType](#nakedexposedtype)
  - [HardDeletableExposedType](#harddeletableexposedtype)
- [Attributes](#attributes)
  - [ExportDataAttribute](#exportdataattribute)
  - [EventBusAttribute](#eventbusattribute)
- [Endpoint Classes](#endpoint-classes)
  - [SourceEndpoint](#sourceendpoint)
  - [DestinationEndpoint](#destinationendpoint)
  - [EndpointSubscriber](#endpointsubscriber)
- [Replication Messages](#replication-messages)
  - [ReplicateDataMessage](#replicatedatamessage)
  - [RefreshMessage](#refreshmessage)
- [Examples](#examples)
- [Configuration](#configuration)

---

## Key Components
- **Exposed Types** – Define entities and relationships targeted for replication.
- **Source and Destination Endpoints** – Handle publication and subscription of replication data.
- **Replication Messages** – Structure the replication data payloads.
- **Attributes** – Declaratively configure replication aspects of entities and endpoints.

---

## ExposedType Classes

### ExposedType\<TDomain\>

Allows exposure of domain entity data to replication mechanism.

#### Main Methods:

- `Define()` – Declares exposed fields and relationships.
- `Expose(...)` – Selectively exposes entity properties.
- `ExposeEverything()` – Quickly exposes all fields for simple scenarios.
- `AddDependency(...)` – Links related entities to synchronize changes.

**Example Usage:**
```csharp
class UserExposedType : ExposedType<User>
{
    public override void Define()
    {
        Expose(u => u.Name);
        Expose(u => u.Email);
        AddDependency(u => u.Department);
    }
}
```

### NakedExposedType

A special version that auto-exposes all fields automatically.

```csharp
class NakedUserType : NakedExposedType<User> { }
```

### HardDeletableExposedType

Overrides default soft-deletion behavior; used when entities must be physically deleted on target endpoints.

---

## Attributes

### ExportDataAttribute

Specifies the export of certain exposed types at an endpoint level.

```csharp
[ExportData(typeof(UserExposedType))]
public class UsersSourceEndpoint : SourceEndpoint { }
```

### EventBusAttribute

Specifies the queue URL where replicated messages are published/subscribed, for different environment configurations.

```csharp
[EventBus(environment: "Production", url: "https://sqs.aws.amazon.com/queue-url")]
public class UsersSourceEndpoint : SourceEndpoint { }
```

---

## Endpoint Classes

### SourceEndpoint

Publishes entity changes/events to replication queues, allowing downstream subscribers to react accordingly.

#### Key Methods:
- `Publish()` – Starts publishing configured entities/events.
- `UploadAll()` – Manually triggers a full replication upload.

### DestinationEndpoint

Consumes replication data from queues and applies changes received from the source to the local database.

#### Key Methods:
- `Subscribe()` – Begins listening for replication messages.
- `PullAll()` – Pulls all pending replication data from configured queues.

### EndpointSubscriber

Internal class used by DestinationEndpoint to deserialize and apply incoming replication messages.

#### Main Functionalities:
- Implement deserialization logic.
- Manage change detection to minimize unnecessary database writes.
- Handle refresh requests and table clears during replication initialization.

---

## Replication Messages

### ReplicateDataMessage

Core payload object, encapsulating entity data changes:
- Includes entity type, serialized data, timestamps, and delete indicators.

```json
{
  "TypeFullName": "MyNamespace.User",
  "Entity": "{ \"ID\": \"123\", \"Name\": \"John Doe\" }",
  "CreationUtc": "2023-01-01T00:00:00Z",
  "ToDelete": false,
  "IsClearSignal": false
}
```

### RefreshMessage

Triggers a full-data refresh or reload of specific entity types:
- Typically used for initialization or data consistency reset.

---

## Examples

### Configuring a Source Endpoint for Replication
```csharp
[ExportData(typeof(UserExposedType))]
[EventBus("Production", "https://sqs.region.amazonaws.com/account/queue")]
public class UserSourceEndpoint : SourceEndpoint { }

var sourceEndpoint = new UserSourceEndpoint();
sourceEndpoint.Publish();
```

### Configuring a Destination Endpoint for Subscription
```csharp
public class UserDestinationEndpoint : DestinationEndpoint
{
    public UserDestinationEndpoint() : base(typeof(User).Assembly)
    {
        Register(typeof(User).FullName);
    }
}

// In startup or configuration method:
var destinationEndpoint = new UserDestinationEndpoint();
await destinationEndpoint.Subscribe();
```

---

## Configuration

### appsettings.json Configuration Example:
```json
{
  "DataReplication": {
    "MyNamespace_UserSourceEndpoint": {
      "Url": "https://sqs.region.amazonaws.com/account/queue"
    },
    "AllowDumpUrl": false,
    "Mode": "MultiServer"
  }
}
```

| Setting | Description |
|---------|-------------|
| **Url** | Defines the replication URL endpoint for queues. |
| **AllowDumpUrl** | Enables or disables manual data dump/refresh URLs. |
| **Mode** | Operation mode (`SingleServer` or `MultiServer`). |

---

## Important Notes

- Entity serialization/deserialization is handled via JSON (Newtonsoft.Json).
- Custom serialization behavior is available by defining CustomExposedFields or overriding serialization defaults.
- Carefully choose attributes and exposed types according to specific application replication rules.
- Handle event subscriptions properly in destination endpoints.

---

## Exception Handling and Logs

Extensive logging is provided for troubleshooting, and detailed exceptions are thrown with meaningful information regarding failed replication attempts.

**Example exception:**
```
Exception: Could not find a field named [FirstName] in [User]
```