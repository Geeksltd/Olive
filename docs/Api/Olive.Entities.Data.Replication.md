# Olive.Entities.Data.Replication

## Overview

Olive Data Replication provides an efficient, reliable, and robust mechanism for synchronizing data between different microservices. By leveraging event-driven architecture and hosted replication queues, Olive ensures that data dependency between services is handled gracefully, improving system resilience and performance.

---

## Table of Contents

- [Introduction](#introduction)
- [Data Replication vs. API-Based Integration](#data-replication-vs-api-based-integration)
- [Essential Components](#essential-components)
  - [ExposedType Classes](#exposedtype-classes)
  - [Endpoint Classes](#endpoint-classes)
    - [SourceEndpoint](#sourceendpoint)
    - [DestinationEndpoint](#destinationendpoint)
  - [Attributes](#attributes)
    - [ExportDataAttribute](#exportdataattribute)
    - [EventBusAttribute](#eventbusattribute)
  - [Replication Messages](#replication-messages)
- [Implementation Example](#implementation-example)
- [Starting the Endpoint Engine](#starting-the-endpoint-engine)
- [Generating Endpoint Proxy Packages](#generating-endpoint-proxy-packages)
- [Advanced Configurations and Tips](#advanced-configurations-and-tips)
- [Exception Handling and Logging](#exception-handling-and-logging)
- [Configuration Options](#configuration-options)
- [Dependencies](#dependencies)

---

## Introduction

In microservices architectures, services often depend on data maintained by other services. Traditionally, APIs were used to share this data, but this approach has limitations around performance, resiliency, and data freshness. Olive Data Replication solves these challenges by replicating relevant data asynchronously between microservices using event-driven queues.

---

## Data Replication vs. API-Based Integration

| Aspect                 | API Approach                                                          | Olive Data Replication Approach                  |
|------------------------|-----------------------------------------------------------------------|--------------------------------------------------|
| **Performance**        | Frequent calls can become slow                                        | Local data access; much faster performance       |
| **Resiliency**         | Consumer outages when publisher is unavailable                        | Replicated data available independent of publisher availability |
| **Data Freshness**     | Caching introduces data lag                                           | Data changes are immediately pushed to consumers |
| **Complexity**         | Cache invalidation and synchronization complexity                     | Simplified, event-driven approach reduces complexity |

---

## Essential Components

### ExposedType Classes

Define and expose entity fields for replication:

- `ExposedType<TDomain>`: explicitly exposes chosen fields.
- `NakedExposedType<TDomain>`: implicitly exposes all fields.
- `HardDeletableExposedType<TDomain>`: allows physical deletion of records on replication targets.

**Example:**

```csharp
public class Customer : ExposedType<Domain.Customer>
{
    public override void Define()
    {
        Expose(x => x.Name);
        Expose(x => x.Email);
    }
}
```

### Endpoint Classes

Endpoints manage replication publishing and subscribing.

#### SourceEndpoint

Publishes entity changes to replication queues, allowing replication subscribers to update their data. Each update triggers an asynchronous message to an event bus queue.

#### DestinationEndpoint

Receives and processes data replication messages, updating the consumer's local data copy.

---

### Attributes

#### ExportDataAttribute

Indicates specific exposed data types to include in a source endpoint for replication.

```csharp
[ExportData(typeof(Customer))]
public class CustomersSourceEndpoint : SourceEndpoint { }
```

#### EventBusAttribute

Defines the event bus queue endpoints for publishing or subscribing.

```csharp
[EventBus("Production", "https://sqs.aws-region.amazonaws.com/account-id/queue.fifo")]
public class CustomersSourceEndpoint : SourceEndpoint { }
```

---

### Replication Messages

- `ReplicateDataMessage`: Contains serialized entity data.
- `RefreshMessage`: Used to initiate a refresh of replicated data from publishers to consumers.

Example message structure:

```json
{
  "TypeFullName": "MyNamespace.Customer",
  "Entity": "{ \"ID\": \"123\", \"Name\": \"Alice\" }",
  "CreationUtc": "2023-01-01T00:00:00Z",
  "ToDelete": false,
  "IsClearSignal": false
}
```

---

## Implementation Example

Consider a "Customer Service" (publisher) exposing customer data and an "Order Service" (consumer) needing customer information to associate with orders.

### Publisher-side Implementation:

```csharp
namespace CustomerService
{    
    [EventBus(Environments.Production, "https://sqs.aws-region.amazonaws.com/account/CustomerService-OrdersEndpoint.fifo")]    
    public class OrdersEndpoint : SourceEndpoint 
    {
        class Customer : ExposedType<Domain.Customer>
        {
            protected override void Define()
            {
                Expose(x => x.Email);
                Expose(x => x.Name);
            }
        }
    }
}
```

---

## Starting the Endpoint Engine

In the `Startup.cs` of the publisher (Customer Service), invoke Publish:

```csharp
public override async Task OnStartUpAsync(IApplicationBuilder app)
{
    await new CustomerService.OrdersEndpoint().Publish();
}
```

In the consumer (Order Service), initiate subscription:

```csharp
public override async Task OnStartUpAsync(IApplicationBuilder app)
{
    await new CustomerService.OrdersEndpoint(typeof(CustomerService.Order).Assembly).Subscribe();
}
```

---

## Generating Endpoint Proxy Packages

Using the `generate-data-endpoint-proxy` tool (a global dotnet tool), you can generate nuget packages for endpoint consumption.

### Tool Installation:

```batch
dotnet tool install -g generate-data-endpoint-proxy
```

### Generating Packages:

```batch
generate-data-endpoint-proxy /assembly:"path\website.dll" /dataEndpoint:Namespace.OrdersEndpoint /out:"c:\generated-packages"
```

Generated packages:

- **CustomerService.OrdersEndpoint**: Reference from consumer service for subscription.
- **CustomerService.OrdersEndpoint.MSharp**: Model definitions for consumer’s data layer.

---

## Advanced Configurations and Tips

### Expose All Fields Automatically (Shortcut):

```csharp
public class Customer : NakedExposedType<Domain.Customer> { }
```

### Custom Fields with Mappings:

```csharp
protected override void Define()
{
    Expose(x => x.Name);
    Expose("Address", x => $"{x.AddressLine1}, {x.City}, {x.PostalCode}");
}
```

### Filtering Exposed Records:

Only expose certain records via overriding filter method:

```csharp
protected override bool Filter(Domain.Customer customer) => customer.Status == CustomerStatus.Approved;
```

For **async filtering**, override `FilterAsync`.

---

## Exception Handling and Logging

- Detailed exceptions for serialization and transmission issues.
- Comprehensive logging available through Olive's built-in logging mechanisms.

Example logged exception message:

```
Exception: Could not serialize field Email on type CustomerService.Domain.Customer
```

---

## Configuration Options

Provide necessary configurations via appsettings.json:

```json
{
  "DataReplication": {
    "Mode": "MultiServer",
    "AllowDumpUrl": true,
    "DumpUrl": "/olive/entities/replication/dump/",
    "CustomerService_OrdersEndpoint": {
      "Url": "https://sqs.aws-region.amazonaws.com/account-id/CustomerService-OrdersEndpoint.fifo"
    }
  }
}
```

| Key            | Description                                         | Default                  |
|----------------|-----------------------------------------------------|--------------------------|
| **Mode**       | Specifies operational mode: SingleServer/MultiServer| Required                 |
| **AllowDumpUrl**| Enables manual refresh URLs (dev only)             | true                     |
| **DumpUrl**    | URL prefix for manual refresh endpoints             | "/olive/…/replication/dump/" |

---

## Dependencies

- **NuGet Packages**:

```bash
Install-Package Olive.Entities
Install-Package Newtonsoft.Json
```

- **Tools**:

```batch
dotnet tool install -g generate-data-endpoint-proxy
```

---

## Data Replication Overall Architecture

![Olive Data Replication Overview](DataReplicationOverview.png)

---

## Important notes:

- Ensure entities can safely handle delete scenarios. Soft deletion is highly recommended.
- Hard deletion requires careful handling of associated entities at consumer endpoints.
- Actions exposed via manual URLs (`/dump/...`) should be restricted to development or secure environments.