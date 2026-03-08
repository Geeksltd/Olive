# Olive.Log.EventBus

## Overview

**Olive.Log.EventBus** is a robust logging implementation tailored for Olive-based microservices, providing infrastructure to seamlessly publish log events onto an Event Bus via message queues. This mechanism enables centralized log collection, real-time monitoring, and easy integration with various logging and monitoring services, ensuring comprehensive, scalable, and unified logging across distributed Olive applications.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Configuration](#configuration)
- [Core Components](#core-components)
  - [`EventBusLogger`](#eventbuslogger-class)
  - [`EventBusLoggerProvider`](#eventbusloggerprovider-class)
  - [`EventBusLoggerOptions`](#eventbusloggeroptions-class)
  - [`EventBusLoggerMessage`](#eventbusloggermessage-class)
- [Usage Examples](#usage-examples)
- [Exception Handling](#exception-handling)
- [Dependencies](#dependencies)
- [Important Notes](#important-notes)

---

## Features

- **Distributed Logging**: Publish log entries directly to an event bus message queue.
- **Batching Support**: Reduces overhead by sending multiple log entries in batches.
- **Centralized Log Collection**: Facilitates easier monitoring and management of logs across multiple microservices.
- **Easy Integration**: Seamless integration into ASP.NET Core logging pipeline using existing Microsoft Logging APIs.
- **Detailed Exception Handling**: Advanced logging of exceptions including stack trace and custom details.

---

## Installation

To enable EventBus Logging in your Olive application, ensure these NuGet packages are installed:

```powershell
Install-Package Olive
Install-Package Olive.Logging
Install-Package Microsoft.Extensions.Logging
```

---

## Configuration

Configure EventBus Logging in your `appsettings.json`:

```json
{
  "Logging": {
    "EventBus": {
      "QueueUrl": "[YOUR_EVENT_BUS_QUEUE_URL]",
      "Source": "[YOUR_MICROSERVICE_NAME]"
    }
  }
}
```

Alternatively, configure via code in your application's startup (`Program.cs` or `Startup.cs`):

```csharp
builder.Logging.AddEventBus(options =>
{
    options.QueueUrl = "[YOUR_EVENT_BUS_QUEUE_URL]";
    options.Source = "[YOUR_MICROSERVICE_NAME]";
});
```

---

## Core Components

### `EventBusLogger` Class

Custom logger implementation extending Olive's batching logging framework.

- **Core method**: `Log(...)` generates log entries forwarded to the event bus.

### `EventBusLoggerProvider` Class

Provides and manages `EventBusLogger` instances and handles sending batched log entries via the Event Bus.

- **Initialization**: Fetches QueueUrl and Source from options or configuration.
- **Method**: `WriteMessagesAsync()` sends batched logs to the configured event bus.

### `EventBusLoggerOptions` Class

Holds configuration options necessary for `EventBusLoggerProvider`.

| Property | Description |
|----------|-------------|
| `QueueUrl` | The URL for the message queue (EventBus) |
| `Source` | Identifies the source microservice or application |

### `EventBusLoggerMessage` Class

Data structure used to send log messages across the event bus.

| Property | Description |
|----------|-------------|
| `Messages` | A collection of log events encapsulated in this batch |
| `Date` | Timestamp of the log batch creation |
| `Source` | The originating service or application |

---

## Usage Examples

### Logging Information

```csharp
public class OrderService
{
    readonly ILogger<OrderService> Logger;

    public OrderService(ILogger<OrderService> logger)
    {
        Logger = logger;
    }

    public void ProcessOrder()
    {
        Logger.LogInformation("Processing a new order.");
        // ...
    }
}
```

### Logging Errors and Exceptions

```csharp
public async Task ExecuteJob()
{
    try
    {
        // Job logic
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Job execution failed.");
    }
}
```

Logs will automatically be captured, batched, and published to your EventBus message queue.

---

## Exception Handling

The `EventBusLoggerProvider` gracefully handles exceptions by logging the error locally (console) and temporarily disabling further event-bus logging (by setting `IsEnabled = false`) upon an unrecoverable error such as message publishing failures:

Example internal logging upon failure:

```
Fatal error: Failed to publish the logs to the event bus.
```

---

## Dependencies

This logger depends on the following packages:

- `Microsoft.Extensions.Logging`
- `Olive`
- `Olive.Logging`

Install via:

```powershell
Install-Package Microsoft.Extensions.Logging
Install-Package Olive
Install-Package Olive.Logging
```

Additionally, ensure connection and network configurations are valid and reliable for your EventBus infrastructure (Amazon AWS SQS, Azure Service Bus, RabbitMQ, etc.).

---

## Important Notes

- **Batch Logging**: Logs are sent as batches to enhance performance and reduce network overhead.
- **Queue Reliability**: Ensure that your Event Bus queues are durable and able to persist log messages reliably.
- **Sensitive Data Protection**: Always ensure your logs do not contain sensitive or confidential data. Mask or sanitize any potentially sensitive information before logging.
- **Performance Considerations**: Adjust batching options and frequency carefully according to your application's logging volume to optimize performance.