# Olive.Aws.EventBus

## Overview
The `EventBusQueue` class provides a structured approach to interacting with Amazon SQS for event-driven message handling. It supports publishing messages, subscribing to events, and managing batch operations.

## Configuration
To use this package, ensure the following configurations are set in your appsettings.json:
- **Aws:EventBusQueue:MaxNumberOfMessages** - The maximum number of messages retrieved in one batch (default: 10).
- **Aws:EventBusQueue:VisibilityTimeout** - The duration (in seconds) messages remain hidden after retrieval (default: 300).

### Installation
To integrate this package into your project, install the necessary dependencies:
```sh
Install-Package AWSSDK.SQS
Install-Package Newtonsoft.Json
```

## AWS Clients
```csharp
public IAmazonSQS Client
```
- **Client**: Provides access to AWS SQS operations.

### `Region(Amazon.RegionEndpoint region)`
```csharp
public EventBusQueue Region(Amazon.RegionEndpoint region)
```
- **Summary**: Creates and uses a new AWS SQS client for the specified region.
- **Usage**:
```csharp
var queue = new EventBusQueue("queue-url").Region(RegionEndpoint.USEast1);
```

## Queue Management

### `Publish`
```csharp
public async Task<string> Publish(string message)
```
- **Summary**: Publishes a message to the queue.
- **Usage**:
```csharp
var queue = new EventBusQueue("queue-url");
string messageId = await queue.Publish("{ \"message\": \"Hello World\" }");
```
- **Notes**:
  - If the queue is FIFO and MessageDeduplicationId or MessageGroupId are empty, the method assigns `MessageDeduplicationId` and `MessageGroupId` automatically.

### `PublishBatch`
```csharp
public async Task<IEnumerable<string>> PublishBatch(IEnumerable<string> messages)
```
- **Summary**: Publishes a batch of messages to the queue.
- **Usage**:
```csharp
var messages = new List<string> { "message1", "message2" };
var messageIds = await queue.PublishBatch(messages);
```
- **Warning**: If too many messages fail, the method retries up to `MAX_RETRY(4)` times.

### `Subscribe`
```csharp
public void Subscribe(Func<string, Task> handler)
```
- **Summary**: Subscribes to the queue and processes incoming messages using the provided handler.
- **Usage**:
```csharp
queue.Subscribe(async message =>
{
    Console.WriteLine($"Received: {message}");
});
```

### `PullAll`
```csharp
public async Task PullAll(Func<string, Task> handler)
```
- **Summary**: Continuously pulls messages from the queue and processes them.
- **Usage**:
```csharp
await queue.PullAll(async message =>
{
    Console.WriteLine($"Processing: {message}");
});
```

### `PullBatch`
```csharp
public async Task<IEnumerable<QueueMessageHandle>> PullBatch(int timeoutSeconds = 10, int? maxNumerOfMessages = null)
```
- **Summary**: Retrieves a batch of messages from the queue.
- **Usage**:
```csharp
var messages = await queue.PullBatch(maxNumerOfMessages : 5);
foreach (var message in messages)
{
    Console.WriteLine(message.RawMessage);
    await message.Complete();
}
```

### `Purge`
```csharp
public Task Purge()
```
- **Summary**: Clears all messages from the queue.
- **Warning**: This action is irreversible.
- **Usage**:
```csharp
await queue.Purge();
```

## Event Bus Provider
The `EventBusProvider` class manages queue instances and caching.

### `Provide`
```csharp
public IEventBusQueue Provide(string queueUrl)
```
- **Summary**: Retrieves an `EventBusQueue` instance for the given queue URL.
- **Usage**:
```csharp
var provider = new EventBusProvider();
var queue = provider.Provide("queue-url");
```

## AWS Event Bus Extensions

### `AddAwsEventBus`
```csharp
public static IServiceCollection AddAwsEventBus(this IServiceCollection @this)
```
- **Summary**: Registers the AWS event bus service with the DI container.
- **Usage**:
```csharp
services.AddAwsEventBus();
```

## SNS Message Handling
The `SnsMessage<T>` and `SnsMessage` classes handle AWS SNS message processing.

### `ParseMessage`
```csharp
public T ParseMessage()
```
- **Summary**: Deserializes the SNS message body into the specified type.
- **Usage**:
```csharp
var snsMessage = new SnsMessage<MyType> { Message = "{ \"name\": \"John\" }" };
var parsedObject = snsMessage.ParseMessage();
```

## Subscriber Class
The `Subscriber` class manages message polling from the queue.

### `Start`
```csharp
public void Start()
```
- **Summary**: Starts polling the queue for messages.

### `PullAll`
```csharp
public Task PullAll()
```
- **Summary**: Fetches and processes all available messages.

## Full Example
```csharp
public override void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddAwsEventBus();
    ...
}  
```

```csharp 
var queueProvider = Context.Current.GetService<IEventBusQueueProvider>();
var queue = queueProvider.Provide("queue-url");

await queue.Publish("Hello, World!");
queue.Subscribe(async message => Console.WriteLine($"Received: {message}"));
```

## Conclusion
The `EventBusQueue` system simplifies AWS SQS operations, providing an efficient mechanism for publishing, consuming, and managing messages. It includes error handling, retries, and supports FIFO queues seamlessly.
