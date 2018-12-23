# Olive EventBus

In distributed systems, different applications often need to send messages to each other. EventBus, provides a publish-subscribe style of communication between components, without requiring the components to explicitly register with one another (and thus be aware of each other).

In Olive, there is a static class named `EventBus` which provides a simple mechanism to achieve that. You will invoke its `Queue(url)` method while providing a queue url, to obtain a `IEventBusQueue` instance. For example:

```csharp
var productPriceQueue = EventBus.Queue(Config.Get("Queues:ProductPrice:Url"));
```

## IEventBusQueue
This interface provides a generic abstraction for publishing and subscribing to events for a particular queue. For production use, you will normally use a modern cloud queue service such as [AWS Simple Queue Service](https://aws.amazon.com/sqs/) or [Azure Queue Storage](https://azure.microsoft.com/en-gb/services/storage/queues/). Alternatively, during development time, you can use a simple file-based implementation to avoid cloud charges.

Olive frameworks comes with a default implementation for AWS SQS as well as an IO based implementation for development and testing. To configure the desired provider, you will register the implementation in `Startup.cs` file. For example:
```csharp
public override void ConfigureServices(IServiceCollection services)
{
    ...
    if (Environment.IsDevelopment())
    {
        services.AddIOEventBus();
    }
    else
    {
        services.AddAwsEventBus();
    }
}
```

### IOEventBus
During development and testing, you can use the `IOEventBusQueue` implementation. It will create a folder in your local machine at `%Temp%\Olive\IO.Queue\{queueFolder}` where `{queueFolder}` is a path safe queue url. For example *sqs.eu-west-1.amazonaws.com_1234_MyQueue*.

Every time you publish a message, a Json file will be created in that folder. When the subscriber handles the message, the file will get deleted. It works based on a first-in-first-out model.


## Publishing a message
This `IEventBusQueue` interface provides simple publish/subscribe functionality. To publish a message, you will invoke the `Publish` method while passing a *message object* which is any object that implements the `IEventBusMessage` interface. Or, more simply, you will implement a messag class that inherits from `EventBusMessage` class. For example:

```csharp
public class ProductPriceChangedMessage : EventBusMessage
{
    public Guid ProductId {get; set;}
    public decimal NewPrice {get; set;}
}
```

In your application logic, where required, you will publish a message. For example:

```csharp
partial class Product
{
    ...
    public override async Task OnSaved()
    {
        if (IsPriceChanged())
        {
             var productPriceQueue = EventBus.Queue(Config.Get("Queues:ProductPrice:Url"));
             await productPriceQueue.Publish(new ProductPriceChangedMessage { ProductId = ID, NewPrice = Price });
        }        
    }
}
```
When you call the `Publish(...)` method, your message object will be serialized and passed to the queue. 

## Subscribing to a message
To receive and handle messages from a queue, you will need to subscribe to the queue. Normally you will write the subscription code in your `Startup.cs` file.

```csharp
public override async Task OnStartUpAsync(IApplicationBuilder app)
{
    ...
    var productPriceQueue = EventBus.Queue(Config.Get("Queues:ProductPrice:Url"));
    productPriceQueue.Subscribe<ProductPriceChangedMessage>(m => ProductPriceChangedHandler.Process(m));
}
```
The handler is a method that takes an instance of the queue message type and retrns a `Task`. For example:
```csharp
public class ProductPriceChangedHandler
{
    public static async Task Process(ProductPriceChangedMessage message)
    {
       ...
    }
}
```

If the method executes without throwig an exception, the message will automatically be deleted from the queue.

## Configuration
The publisher and subscriber microservices need to share the same queue url and message schema.
They do not have to reference each other directly, or in fact have any knowledge of each other.

For example you can add the following to both applications' `appSettings.json` files:

```json
{
   ...
   "Queues": {
      "ProductPrice": {
         "Url": "https://....{queue url}..."
      }
   }
}
```

Note: The queue url can be the same for development as well as production environments. When you configure the `IOEventBus` in your `Startup.cs` file, it will just create a folder with that URL to simulated the queue.
