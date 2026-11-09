# Olive.Entities.Data.Replication.Mvc.Extensions

## Overview

The Olive Entities Data Replication provides extension methods to seamlessly integrate Olive's data replication system into ASP.NET Core applications. It simplifies registering and configuring replication sources (publishers) and destinations (subscribers) within your application's middleware pipeline.

---

## Table of Contents
- [Key Components](#key-components)
- [Extensions Methods](#extension-methods)
  - [RegisterSubscriber](#registersubscriber)
  - [RegisterPublisher](#registerpublisher)
- [Examples](#examples)
- [Configuration Options](#configuration-options)

---

## Key Components

The provided extension methods handle common tasks:

- **RegisterSubscriber**: Sets up a replication subscriber endpoint, enabling data synchronization from external sources.
- **RegisterPublisher**: Configures a replication publisher endpoint that can expose data to other subscribed applications.

---

## Extension Methods

### RegisterSubscriber

Registers a replication subscriber (destination endpoint), setting up automatic data pulling from configured replication queues.

**Syntax:**

```csharp
public static IApplicationBuilder RegisterSubscriber<T>(this IApplicationBuilder app, Assembly domainAssembly)
    where T : DestinationEndpoint
```

**Features:**

- In 'MultiServer' mode, registers a `BackgroundJob` to periodically pull updates every minute (`* * * * *`).
- In 'SingleServer' mode, directly subscribes to replication events on application startup.
- Sets up an HTTP mapping (`/olive-endpoints/{endpoint}`) allowing manual triggering of replication.

**Usage:**

```csharp
app.RegisterSubscriber<MyDestinationEndpoint>(typeof(MyEntity).Assembly);
```

---

### RegisterPublisher

Registers a replication publisher (source endpoint), enabling entities to be pushed and refreshed from publisher applications.

**Syntax:**

```csharp
public static IApplicationBuilder RegisterPublisher(this IApplicationBuilder app, SourceEndpoint endpoint);
public static IApplicationBuilder RegisterPublisher<TSourceEndpoint>(this IApplicationBuilder app) where TSourceEndpoint : SourceEndpoint, new();
```

**Features:**

- Publishes changes for registered entities on the replication event bus.
- Optionally registers dump URLs for manual data uploading and refreshing.
- Provides a summary URL (`/olive/entities/replication/dump/all`) to list all registered endpoints for debugging or manual triggers.

**Usage:**

```csharp
app.RegisterPublisher<MySourceEndpoint>();
```

or with a created instance:

```csharp
var myEndpoint = new MySourceEndpoint();
app.RegisterPublisher(myEndpoint);
```

---

## Examples

### 1. Configuring a Subscriber Endpoint

In Startup.cs, `Configure` method:

```csharp
app.RegisterSubscriber<MyDestinationEndpoint>(typeof(MyEntity).Assembly);
```

### 2. Configuring a Publisher Endpoint

In Startup.cs, `Configure` method:

```csharp
 app.RegisterPublisher<MySourceEndpoint>();
```

Creating custom publisher instance with specific configurations:

```csharp
var publisher = new CustomSourceEndpoint();
app.RegisterPublisher(publisher);
```

---

## Configuration Options

### AppSettings Configuration (`appsettings.json`):

```json
"DataReplication": {
    "Mode": "MultiServer",
    "AllowDumpUrl": true,
    "DumpUrl": "/olive/entities/replication/dump/"
}
```

| Option | Description                                                                                 | Default                             |
|--------|---------------------------------------------------------------------------------------------|-------------------------------------|
| Mode   | Defines mode: Set "MultiServer" for distributed scenarios or "SingleServer" otherwise.      | **Required** (no default value)     |
| AllowDumpUrl | Enables the Dump URLs to refresh or upload all data via browser or manual HTTP request. | true                                |
| DumpUrl      | Base URL prefix for exposing refresh endpoints.                                         | /olive/entities/replication/dump/   |

---

## Important Notes

- Ensure correct configuration of replication modes (`MultiServer` or `SingleServer`) before deployment.
- Dump URL refresh actions should typically remain enabled only in development or secured environments due to their sensitive nature.
- Logs are generated for each action and request, providing detailed audit trails and debugging information.

---

## Exception Handling

Configuration missteps, such as omitting `"DataReplication:Mode"`, will raise exceptions at runtime startup. Clearly defined exception messages guide straightforward configuration troubleshooting.

Example exception messages:

```text
Exception: DataReplication:Mode has not been specified. Options: MultiServer, SingleServer
```