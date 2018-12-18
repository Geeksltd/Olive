# Olive EventBus

In distributed systems, different applications often need to send messages to each other. EventBus, provides a publish-subscribe style of communication between components, without requiring the components to explicitly register with one another (and thus be aware of each other).

In Olive, there is a static class named `EventBus` which provides a simple mechanism to achieve that. You will invoke its `Queue(url)` method while providing a queue url, to obtain a `IEventBusQueue` instance. For example:

```c#
var productQueue = EventBus.Queue(Config.Get("Queues:Product:Url"));
```

## IEventBusQueue
This interface provides simple publish/subscribe functionality. To publish a message, you will invoke the `Publish` method while passing a *message object* which is any object that implements the `IEventBusMessage` interface. For example:

```c#

```

