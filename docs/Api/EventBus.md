# Olive EventBus

In distributed systems, different applications often need to send messages to each other. EventBus, provides a publish-subscribe style of communication between components, without requiring the components to explicitly register with one another (and thus be aware of each other).

In Olive, there is a static class named `EventBus` which provides a simple mechanism to achieve that. You will invoke its `Queue(url)` method while providing a queue url, to obtain a `IEventBusQueue` instance. For example:

```c#
var productPriceQueue = EventBus.Queue(Config.Get("Queues:ProductPrice:Url"));
```

## Publishing a message
This `IEventBusQueue` interface provides simple publish/subscribe functionality. To publish a message, you will invoke the `Publish` method while passing a *message object* which is any object that implements the `IEventBusMessage` interface. Or, more simply, you will implement a messag class that inherits from `EventBusMessage` class. For example:

```c#
public class ProductPriceChangedMessage : EventBusMessage
{
    public Guid ProductId {get; set;}
    public decimal NewPrice {get; set;}
}
```

In your application logic, where required, you will publish a message. For example:

```c#
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
