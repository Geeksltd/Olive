# Olive RabbitMQ Event Bus Library Documentation

This document provides an overview and usage examples for the public classes and methods in the `Olive.RabbitMQ` namespace. It provides an event bus implementation using RabbitMQ for publishing and consuming messages, supporting both single and batch operations. A configuration section details the required settings in an `appsettings.json` file.

---

## Table of Contents

1. [EventBusProvider](#eventbusprovider)
   - [Overview](#eventbusprovider-overview)
   - [Methods](#eventbusprovider-methods)
2. [EventBusQueue](#eventbusqueue)
   - [Overview](#eventbusqueue-overview)
   - [Methods](#eventbusqueue-methods)
3. [RabbitMQEventBusExtensions](#rabbitmqeventbusextensions)
   - [Overview](#rabbitmqeventbusextensions-overview)
   - [Methods](#rabbitmqeventbusextensions-methods)
4. [Configuration](#configuration)

---

## EventBusProvider

### EventBusProvider Overview

The `EventBusProvider` class implements `IEventBusQueueProvider` to provide instances of `EventBusQueue`, caching them by queue URL for efficient reuse.

### EventBusProvider Methods

- **`Provide(string queueUrl)`**
  - Returns an `IEventBusQueue` instance for the specified queue URL, creating a new one if not cached.
  - **Usage Example:**
    ```csharp
    var provider = new EventBusProvider();
    var queue = provider.Provide("my-queue");
    await queue.Publish("Hello, RabbitMQ!");
    ```

---

## EventBusQueue

### EventBusQueue Overview

The `EventBusQueue` class implements `IEventBusQueue` to interact with a RabbitMQ queue, supporting message publishing, batch publishing, subscription, and pulling messages.

### EventBusQueue Methods

- **`Publish(string message)`**
  - Publishes a single message to the queue.
  - **Usage Example:**
    ```csharp
    var queue = new EventBusQueue("my-queue");
    await queue.Publish("Single message");
    ```

- **`PublishBatch(IEnumerable<string> messages)`**
  - Publishes a batch of messages to the queue.
  - **Usage Example:**
    ```csharp
    var queue = new EventBusQueue("my-queue");
    var messages = new[] { "Message 1", "Message 2", "Message 3" };
    await queue.PublishBatch(messages);
    ```

- **`Subscribe(Func<string, Task> handler)`**
  - Subscribes to the queue with a handler function that processes incoming messages.
  - **Usage Example:**
    ```csharp
    var queue = new EventBusQueue("my-queue");
    queue.Subscribe(async message =>
    {
        Console.WriteLine($"Received: {message}");
        await Task.CompletedTask;
    });
    ```

- **`PullAll(Func<string, Task> handler)`**
  - Pulls all messages from the queue and processes them with the provided handler until the queue is empty.
  - **Usage Example:**
    ```csharp
    var queue = new EventBusQueue("my-queue");
    await queue.PullAll(async message =>
    {
        Console.WriteLine($"Pulled: {message}");
        await Task.CompletedTask;
    });
    ```

- **`PullBatch(int timeoutSeconds = 10, int? maxNumerOfMessages = null)`**
  - Pulls a batch of messages from the queue with a specified timeout and optional maximum number.
  - **Usage Example:**
    ```csharp
    var queue = new EventBusQueue("my-queue");
    var messages = await queue.PullBatch(timeoutSeconds: 5, maxNumerOfMessages: 5);
    foreach (var msg in messages)
    {
        Console.WriteLine($"Batch message: {msg.RawMessage}");
        await msg.Complete();
    }
    ```

- **`Pull(int timeoutSeconds = 10)`**
  - Pulls a single message from the queue with a specified timeout.
  - **Usage Example:**
    ```csharp
    var queue = new EventBusQueue("my-queue");
    var message = await queue.Pull(timeoutSeconds: 5);
    if (message != null)
    {
        Console.WriteLine($"Single pull: {message.RawMessage}");
        await message.Complete();
    }
    ```

- **`Purge()`**
  - Purges all messages from the queue.
  - **Usage Example:**
    ```csharp
    var queue = new EventBusQueue("my-queue");
    await queue.Purge();
    Console.WriteLine("Queue purged");
    ```

---

## RabbitMQEventBusExtensions

### RabbitMQEventBusExtensions Overview

The `RabbitMQEventBusExtensions` static class provides an extension method to register the RabbitMQ event bus provider in an ASP.NET Core dependency injection container.

### RabbitMQEventBusExtensions Methods

- **`AddRabbitMQEventBus(this IServiceCollection @this)`**
  - Registers `EventBusProvider` as the `IEventBusQueueProvider` in the service collection.
  - **Usage Example:**
    ```csharp
 
    services.AddRabbitMQEventBus();
 
    var eventBusProvider = Context.Current.GetService<IEventBusQueueProvider>();
    ```

---

## Configuration

The `EventBusQueue` class relies on specific configuration settings stored in an `appsettings.json` file with a JSON structure. Below are the required and optional settings:

### Required Settings
- **`RabbitMQ:Host`**
  - The hostname of the RabbitMQ server.
- **`RabbitMQ:Username`**
  - The username for RabbitMQ authentication.
- **`RabbitMQ:Password`**
  - The password for RabbitMQ authentication.

### Optional Settings
- **`RabbitMQ:Port`** (Default: `5672`)
  - The port number for the RabbitMQ connection.
- **`RabbitMQ:EnableSSL`** (Default: `false`)
  - Whether to enable SSL for the RabbitMQ connection.
- **`RabbitMQ:EventBusQueue:MaxNumberOfMessages`** (Default: `10`)
  - The maximum number of messages to pull in a batch (1-10).
- **`RabbitMQ:EventBusQueue:VisibilityTimeout`** (Default: `300`)
  - The duration (in seconds) that received messages are hidden from subsequent retrievals.

### Full `appsettings.json` Example
```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest",
    "Port": 5672,
    "EnableSSL": false,
    "EventBusQueue": {
      "MaxNumberOfMessages": 5,
      "VisibilityTimeout": 600
    }
  }
}
```

### Notes
- If any required settings (`Host`, `Username`, `Password`) are missing, the `EventBusQueue` instantiation will fail unless overridden programmatically.
- When `EnableSSL` is `true`, the connection uses TLS 1.1 or 1.2, and server certificate validation is bypassed for simplicity (not recommended for production without proper certificate handling).
