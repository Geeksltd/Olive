# Intra-service Communication 

In a monolithic .NET app running on a single process, components invoke one another using simple method calls. That is obviously not applicable across microservices that run in separate processor or even servers.

You could replace that with some form of remote procedure calls (RPC) such as Web APIs. But a direct conversion from in-process method calls into RPC will cause a **chatty and inefficient** communication. Due to overheads in making each single remote call, that style will not perform well in distributed environments. You can learn more about it at [he fallacies of distributed computing](https://en.wikipedia.org/wiki/Fallacies_of_distributed_computing).

One solution is to replace many fine-grained calls with few coarser-grained ones. You can do this by grouping calls, and by returning data that aggregates the results of multiple internal calls, to the consumer.

## Sync vs Async communication
The goal of each microservice is to be autonomous and available to the client consumer, even if the other services that are part of the end-to-end application are down or unhealthy. If you need to make a call from one microservice to other microservices and await its response before you are able to provide a response to the user, you have an architecture that will not be resilient when some microservices fail. 

### Http: Synchronous protocol
HTTP is a synchronous protocol. One sends a request and waits for a response from the service. Note that this is independent and different from the client code's thread execution that could be sync or async (ie thread is blocked or not), as that's a different concept. The important point here is that the protocol (HTTP/HTTPS) is synchronous and the client code can only continue its task when it receives the HTTP server response. 

### AMQP: Asynchronous protocol
AMQP is a protocol supported by many operating systems and cloud environments which uses asynchronous messages. In this model the client code sends a message but does not wait for a response. Usually the wiring technology, such as RabbitMQ, will handle retries, addressing, or if needed broadcasting to multiple recipients.

A microservice-based application will often use a combination of both of these styles, with the most common scenario being direct synchronous HTTP calls to Web APIs.

### Asynchronous integration facilitates microservice autonomy 
As a rule of thumb choose Async over Sync whenever possible. That does not mean that you have to use a specific protocol (for example, AMQP messaging versus HTTP). It just means that the communication between microservices should be done only by propagating data asynchronously, but try not to depend on other internal microservices as part of the initial service's HTTP request/response operation. 

HTTP dependencies between microservices, like when creating long request/response cycles with HTTP request chains, is bad news for your services' autonomy as well as performance. The more you add synchronous dependencies between microservices, such as query requests, the worse the overall response time gets for the user. 

![Sync vs Async](https://user-images.githubusercontent.com/1321544/50510690-eb950300-0a9f-11e9-9827-6745208e43ec.jpg)

If your microservice needs to raise an additional action in another microservice, if possible, do it asynchronously queue based events.

### Async data replication
If your in microservice needs data that is originally owned by another microservices, do not rely on making sync requests to get it. Instead, replicate that data (only the attributes you need) in and keep it up to date using integration events, as explained later.

Note that as explained earlier, duplicating some data across several microservices (bounded contexts) is not an incorrect design. On the contrary, when doing that you can translate the data into the specific language or terms of that additional domain or Bounded Context. 

You might use any protocol to communicate and propagate data asynchronously across microservices, such as event/queue based messaging or regular HTTP polling.

## Message/queue based communication
When using messaging, processes communicate by exchanging messages asynchronously. A client makes a command or a request to a service by sending it a message. If the service needs to reply, it sends a different message back to the client. Since it is a message-based communication, the client assumes that the reply will not be received immediately, and that there might be no response at all. 

Messages are usually sent through asynchronous protocols like AMQP via a lightweight message broker, such as RabbitMQ.

Also note that this communication style is best suited for internal services. For publicly published APIs you should use synchronous Http based Web Api.

### Async event-driven communication
When using asynchronous event-driven communication, a microservice publishes an integration event when something happens within its domain and another microservice needs to be aware of it, like a price change in a product catalog microservice. 

Interested other microservices subscribe to the events so they can receive them asynchronously. When that happens, the receivers might update their own domain entities, which can cause more integration events to be published.

For orchestration and reliability, this publish/subscribe system is usually implemented via an **event bus** or **messaging broker** such as **RabbitMQ**.

#### Multiple message receivers
Most Async messages are commands sent from one microservice to another. 

But there are cases when a message should be sent to multiple receivers. In those cases you can use a publish/subscribe mechanism using **event driven** communication. That way, **additional subscribers can be added in the future** without the need to modify the sender service. 

When you use a publish/subscribe communication, you might be using an event bus interface to publish events to any subscriber.

### Strict consistency vs Eventual Consistency
Strict consistency means that when something happens, it has happened from everyone's point of view. There is a single source of truth (usually a relational database). ACID transactions are used to implement this using locking and other means so no one can have an inconsistent version of the truth even when complex changes or transactions are half way and with yet an uncertain fate (commit vs rollback).

Strict consistency (via ACID compliance) makes the world a simpler place. For example:
Your bank balance is $50.


- You deposit $100.
- Your bank balance, queried from any ATM anywhere, is $150.
- Your daughter withdraws $40 with your ATM card.
- Your bank balance, queried from any ATM anywhere, is $110.
- At no time can your balance reflect anything other than the actual sum of all of the transactions made on your account to that exact moment.

The problem with Strict Consistency is limited scalability and availability, which goes against the essence of microservices and distributed system. To combat this problem there is the idea of Eventual Consistency. It works like this:

1. I watch the weather report and learn that it's going to rain tomorrow.
2. I tell you that it's going to rain tomorrow. Your neighbor tells his wife that it's going to be sunny tomorrow.
3. You tell your neighbor that it is going to rain tomorrow.
4. Eventually, all of the servers (you, me, your neighbor) know the truth (that it's going to rain tomorrow), but in the meantime the client (his wife) came away thinking it is going to be sunny, even though she asked after one or more of the servers (you and me) had a more up-to-date value.

Eventual consistency is scalable and provides autonomy and independence to each microservice. It's often implemented through event based messaging as you will learn later. 

If you use eventual consistency make it **completely clear to the end user**. The end user and the business owner have to explicitly embrace eventual consistency in the system. Often a business does not have any problem with this approach, as long as it is explicit. This is important because users might expect to see some results immediately and this might not happen with eventual consistency.

> Note: You can't use Eventual Consistency when the transactions and real-time consistency are required for the application. To use Eventual Consistency, the business processes often need to be changed to become tolerant of inconsistencies. This means that the business has to buy into this concept and build processes for manual corrections.

An eventually consistent transaction is made up of a collection of distributed actions across different microservices.At each action, the related microservice updates a domain entity and publishes another integration event that raises the next action within the same end-to-end business task. You will learn how to implement this using RabbitMQ and MassTransit later.  

![Async event-driven](https://user-images.githubusercontent.com/1321544/50511002-12a00480-0aa1-11e9-9c2b-b3b7346b6c27.jpg)

### Integration events 
Integration events are used for brining data changes from one micro service to one or more other microservices. When an event is published, an appropriate event handler in each receiver microservice handles the event.  

An integration event is basically a data-holding class defined in the publishing microservice, as in the following example: 

```csharp
public class ProductPriceChangedIntegrationEvent : IntegrationEvent 
{         
    public int ProductId { get; private set; } 
    public decimal NewPrice { get; private set; }  
    public decimal OldPrice { get; private set; }
} 
```

### The event bus
An event bus allows publish/subscribe-style communication between microservices without requiring the components to explicitly be aware of each other. This brings decoupling and autonomy to each microservice and eliminates the need for a direct dependency between them. From an application point of view, the event bus is nothing more than a Pub/Sub channel. 

![Event bus](https://user-images.githubusercontent.com/1321544/50513634-29992380-0aae-11e9-9255-5d19540434ac.jpg)

## Implementing an event bus with RabbitMQ
You can create a custom event bus based on RabbitMQ running in a container.

![RabbitMQ](https://user-images.githubusercontent.com/1321544/50513661-6f55ec00-0aae-11e9-9c1c-f4a983056b77.jpg)

The following simplified implementation gets a connection and channel to RabbitMQ, creates a message, and then publishes the message into the queue.

```csharp
public class EventBusRabbitMQ
{ 
    // ...
    public void Publish(IntegrationEvent @event)
    { 
        var eventName = @event.GetType().Name; 
        var factory = new ConnectionFactory() { HostName = _connectionString };         

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel()) 
        { 
            channel.ExchangeDeclare(exchange: _brokerName, 
                         type: "direct"); 
 
            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message); 
 
            channel.BasicPublish(exchange: _brokerName,
                                 routingKey: eventName,  
                                 basicProperties: null,
                                 body: body);                 
        }     
    } 
} 
```

A better implementation should retry the task a number of times in case the RabbitMQ container is not ready yet. This can occur, for example, when the RabbitMQ container starts more slowly than the app container.

### Implementing the subscription code
In RabbitMQ each event type has a related channel. You can have any number of event handlers per channel. The following is a simplified implementation for event handler subscription:

```csharp
public class EventBusRabbitMQ
{ 
    // ... 
 
    public void Subscribe<T>(Action<T> handler)        
           where T : IntegrationEvent 
    { 
        var eventName = _subsManager.GetEventKey<T>(); 
         
        var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);          
       
        if (!containsKey) 
        { 
            if (!_persistentConnection.IsConnected) 
                _persistentConnection.TryConnect(); 
 
            using (var channel = _persistentConnection.CreateModel()) 
            { 
                channel.QueueBind(queue: _queueName, 
                                  exchange: BROKER_NAME,
                                  routingKey: eventName); 
            }
             } 
 
        _subsManager.AddSubscription(handler); 
    } 
} 
```

The Subscribe method accepts an event handler Action delegate, which is invoked every time an event message is received in the channel.

### Subscribing to events
The following code shows what each receiver microservice needs to implement when starting (in the Startup class) in order to subscribe to an event.

In this example, the basket microservice needs to subscribe to **ProductPriceChanged** event messages so that it's aware of any changes to the product price, so it can warn the user..  

```csharp
Olive.EventBus.Current.Subscribe<ProductPriceChanged>(OnPriceChanged);

void OnPriceChanged(ProductPriceChanged data)
{
   […]
}
```

### Publishing events through the event bus 
Finally, the message sender (origin microservice) publishes the events similar to the following example:

```csharp
Olive.EventBus.Current.Publish(new ProductPriceChanged(...));
```

You would implement something like this usually right after committing data change transactions. 

## Atomicity and resiliency
When you publish integration events you need to atomically update the original database and publish an event (that is, either both operations complete or none of them).

To achieve that you can use the [Outbox pattern](http://gistlabs.com/2014/05/the-outbox/). Basically, you have an IntegrationEvent table in the same database where you have your domain entities. That table works as an insurance for achieving atomicity so that you include persisted integration events into the same transactions that are committing your domain data. 

The process goes like this: 

1. The application begins a local database transaction.
2. It then updates the state of your domain entities. 
3. It also inserts an event into the integration event table. 
4. Finally, it commits the transaction.

You will also have a background scheduled task to actually publish the events:

1. It queries the integration event table for new unprocessed items
2. It publishes each one to RabbitMQ
3. It then marsk the events as published. 

![Atomicity and resiliency](https://user-images.githubusercontent.com/1321544/50513820-5f8ad780-0aaf-11e9-9465-825780a06307.jpg)

### Message idempotency
Idempotency means that an operation can be performed multiple times without changing the result. In a messaging environment, an event is idempotent if it can be delivered multiple times without changing the result for the receiver microservice.

This is necessary because of the nature of distributed systems. In general in distributed systems when a remote call is made to publish a message, if the sender receives a positive acknowledgement things are easy. But if it doesn't receive an acknowledgement (for example due to a network failure), it has no way of knowing if the receiver has received the message or not. So it has no option but to implement a retry mechanism. 

This means that on the receiving end **the same message might be received multiple times**. 

### Handling duplicate events
Sometimes this is not a problem to receive and process a duplicate message. For example, if an event subscriber generates image thumbnails, processing the same message multiple times is not problematic as ultimately the same thumbnail is generated every time. 

On the other hand, operations such as calling a payment gateway to charge a credit card will cause trouble and you need to ensure that receiving the same event message multiple times is correctly discarded. The best way to deal with this issue is to have a unique identity per event message so that you can write basic logic to ensure that each unique event ID is processed only once. 

### Deduplicate messages from RabbitMQ 
According to the [RabbitMQ documentation](https://www.rabbitmq.com/reliability.html#consumer), "If a message is delivered to a consumer and then requeued (because it was not acknowledged before the consumer connection dropped, for example) then RabbitMQ will set the redelivered flag on it when it is delivered again. 

If the "redelivered" flag is set, the receiver must take that into account, because the message might already have been processed. But that is not guaranteed; the message might never have reached the receiver after it left the message broker, perhaps because of network issues. On the other hand, if the "redelivered" flag is not set, it is guaranteed that the message has not been sent more than once. Therefore, the receiver needs to deduplicate messages or process messages in an idempotent way only if the "redelivered" flag is set in the message. 

## Api Input / output types
The domain model entities in each microservice are private to that service. When you create Web Apis that need to send or receive structured data, the best approach is to define simple DTO (Data Transfer Object) classes dedicated to each Api client, based on exactly what they need. They are also sometimes referred to as View Model classes.

The Api implementation can of course initially run database queries based on its real domain entities, to obtain the data needed. But rather than sending the domain objects to the client directly, it should create the View Model objects to return (usually after Json serialization by the ASP.NET framework). 

The returned data (ViewModel) can be the result of joining data from multiple entities or tables in the database. This approach provides great flexibility and also decoules client services from your domain model design, making it easier to change things without worrying about breaking other applications. 

## Api versioning
A microservice API is a contract between the service and its clients. You will be able to evolve a microservice independently only if you do not break its API contract (schema), which is why the contract is so important. If you change the contract, it will impact your client applications or your API Gateway.

Regardless of how thoughtful you were when designing your initial contract, it will need to change over time.

### Backward compatible changes
Sometimes changes are small and in a way that you can write your api implementation that it can be backward compatible. For example if add new parameters to your API, you might be able to provide default values for any missing attributes that are required. Or if you are adding new attributes to the response objects, the clients might be able to ignore any extra response attributes.

But if the nature of your changes are not automatically backward compatible then you need to decide if you must provide different versions of your Api.

### Public APIs
If your API is used by external applications made by other people, you cannot force all clients to upgrade to your new API contract.

You usually need to incrementally deploy new versions of a service in a way that both old and new versions of a service contract are running simultaneously, at least for a period of time. 

### Internally used APIs: Do you need versioning?
If your changed API is designed for and dedicated to only internal consumer microservices which are owned and controlled by you, it may be possible to force them to also be updated to use the new version of the Api. That way you don't have to get into the trouble of maintaining multiple api versions. 

But if the consuming microservices have a different release cycle, for example when they are under active development for other changes of their own, this may not be possible to deploy the new version of all services at the same time. This means that you may need to support multiple API versions at the same time. 

### Multiple live Api versions
When you do need to support multiple API versions, a common pattern for RESTful http services is to embed the version number in the api url:

- My-app.com/api/v1/my-api
- My-app.com/api/v2/my-api

Your latest API version normally needs a full implementation anyway. But for your older API version(s) you have two options. One option is to keep it intact, so it will also have a full implementation. However in this way you need to also maintain it, apply bug fixes, security updates and so on like any other production code. It can lead to code duplication and maintenance problems.

### Lightweight adapter implementation
Alternatively you can change the old API's implementation to merely delegate the call to the new version, while adding the compatibility logic. This helps you avoid implementation code duplication.

Your latest API will be the only one who implements the actual thing, while your old API implementation will only contain the logic to make an old request compatible with the new contract, and also convert the response to the old contract's format.

## Api documentation with Swagger
[Swagger](https://swagger.io/) is an open-source framework for documenting APIs for both humans and the machine. The Swagger specification is the basis of the OpenAPI Specification (OAS) to standardize the way RESTful interfaces are defined. It's supported by Microsoft as well as all other key vendors.

There is a rich ecosystem of [libraries and frameworks](https://swagger.io/tools/open-source/open-source-integrations/) across all platforms that support Swagger. For example, [AutoRest](https://github.com/Azure/AutoRest) automatically generates .NET client classes. Other tools like [swagger-codegen](https://github.com/swagger-api/swagger-codegen) are also available.

By adding Swagger to your APIs, you open up to the Swagger ecosystem, community, tools and libraries, and enable potential consumers to more easily integrate with you. 

### Generate API Swagger metadata with Swashbuckle (NuGet package)
Generating Swagger metadata Json file manually would be tedious. However, you can use the [Swashbuckle NuGet package](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) to dynamically generate Swagger API metadata from your Asp.net Web Api. It is added by default in the M# project template. You can request the following url to see the generated swagger metadata:

```csharp
http://<your-root-url>/swagger/v1/swagger.json
```

In addition it creates a nice web based UI for your API, similar to the following when you request:

```csharp
http://<your-root-url>/swagger/
```

![swagger](https://user-images.githubusercontent.com/1321544/50514090-4125db80-0ab1-11e9-9f57-32074a702ff7.jpg)

