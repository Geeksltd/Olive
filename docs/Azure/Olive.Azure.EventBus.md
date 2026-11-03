**Olive.Azure.EventBus**

## Overview
The `Olive.Azure.EventBus` is designed to interact with Azure Service Bus, providing functionalities for publishing, subscribing, and retrieving messages from a queue. This implementation ensures reliable message handling with configurable limits.

---

## **EventBusQueue Class**

### **Properties**
- `MaxNumberOfMessages`: Defines the maximum number of messages to return from the queue. Defaults to `10`.
- `QueueUrl`: Stores the queue’s URL.
- `IsFifo`: Indicates if the queue is FIFO-based.
- `Limiter`: A rate limiter to control message publishing frequency.

### **Methods**

#### **`EventBusQueue(string queueUrl)`**
- Initializes a new instance of the `EventBusQueue` class.
- **Parameters:**
  - `queueUrl` *(string)*: The URL of the Azure Service Bus queue.
- **Usage:**
  ```csharp
  var queue = new EventBusQueue("your-queue-url");
  ```

#### **`Task<string> Publish(string message)`**
- Publishes a single message to the Azure Service Bus queue.
- **Parameters:**
  - `message` *(string)*: The message content to be sent.
- **Returns:**
  - `Task<string>`: The message ID of the published message.
- **Usage:**
  ```csharp
  string messageId = await queue.Publish("Hello, Azure Service Bus!");
  ```

#### **`Task<IEnumerable<string>> PublishBatch(IEnumerable<string> messages)`**
- Publishes a batch of messages to the queue.
- **Parameters:**
  - `messages` *(IEnumerable<string>)*: Collection of messages to be sent.
- **Returns:**
  - `Task<IEnumerable<string>>`: A collection of message IDs.
- **Usage:**
  ```csharp
  var messageIds = await queue.PublishBatch(new List<string>{ "Msg1", "Msg2" });
  ```

#### **`void Subscribe(Func<string, Task> handler)`**
- Subscribes to the queue to process incoming messages.
- **Parameters:**
  - `handler` *(Func<string, Task>)*: A function to process incoming messages.
- **Usage:**
  ```csharp
  queue.Subscribe(async message => Console.WriteLine("Received: " + message));
  ```

#### **`Task<IEnumerable<QueueMessageHandle>> PullBatch(int timeoutSeconds = 10, int? maxNumerOfMessages = null)`**
- Retrieves a batch of messages from the queue.
- **Parameters:**
  - `timeoutSeconds` *(int, optional)*: Timeout for retrieving messages.
  - `maxNumerOfMessages` *(int?, optional)*: Maximum number of messages to pull.
- **Returns:**
  - `Task<IEnumerable<QueueMessageHandle>>`: Collection of queue message handles.
- **Usage:**
  ```csharp
  var messages = await queue.PullBatch();
  ```

#### **`Task Purge()`**
- Deletes all messages from the queue.
- **Usage:**
  ```csharp
  await queue.Purge();
  ```

---

## **Subscriber Class**
The `Subscriber` class handles polling and message processing from an `EventBusQueue`.

### **Methods**

#### **`Subscriber(EventBusQueue queue, Func<string, Task> handler)`**
- Initializes a new instance of the `Subscriber` class.
- **Parameters:**
  - `queue` *(EventBusQueue)*: The event bus queue instance.
  - `handler` *(Func<string, Task>)*: Function to process received messages.

#### **`void Start()`**
- Starts the polling process for retrieving messages.

#### **`Task PullAll()`**
- Retrieves all available messages from the queue.

---

## **EventBusProvider Class**
The `EventBusProvider` manages event bus queue instances.

### **Methods**
#### **`IEventBusQueue Provide(string queueUrl)`**
- Retrieves or creates a queue instance.
- **Usage:**
  ```csharp
  var provider = new EventBusProvider();
  var queue = provider.Provide("your-queue-url");
  ```

---

## **AzureMessagingContext Class**
Handles Azure Service Bus client interactions.

### **Properties**
- `Sender`: Provides a `ServiceBusSender` for publishing messages.
- `Receiver`: Provides a `ServiceBusReceiver` for retrieving messages.
- `Purger`: Provides a `ServiceBusReceiver` for purging messages.

### **Methods**
#### **`AzureMessagingContext(string queueUrl)`**
- Initializes a messaging context for a given queue.
- **Usage:**
  ```csharp
  using var context = new AzureMessagingContext("your-queue-url");
  ```

---

## **AzureEventBusExtensions Class**
Provides an extension method for registering Azure event bus services.

### **Methods**
#### **`IServiceCollection AddAzureEventBus(this IServiceCollection services)`**
- Registers `IEventBusQueueProvider` with the DI container.
- **Usage:**
  ```csharp
  services.AddAzureEventBus();
  ```

---

## **Warnings & Notes**
- Ensure your Azure Service Bus connection string is correctly configured.
- **Purge method should be used with caution** as it will delete all messages in the queue.
- FIFO queues should be handled carefully to maintain ordering.
- Implement proper exception handling when processing messages.

---

## **Conclusion**
The `EventBusQueue` provides a robust solution for integrating Azure Service Bus into your applications. With capabilities for message publishing, batch processing, and automatic message polling, it simplifies event-driven communication in distributed systems.

