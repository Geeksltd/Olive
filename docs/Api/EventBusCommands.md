# EventBus Commands

Often a microservice should invoke a command in another microservice.

This is somewhat similar to sending POST, PUT or DELETE http messages to APIs, without expecting a return value.
However, the benefit of using a queue-based approach is that it's more **reliable** and **scalable**.

## Error handling
When you invoike a Web API synchronously, if it fails, you will know straight away, and can perhaps show a message to the user.
But with a queue-based approach, things are different. As the calling service will simply write a message in the queue, it's almost guaranteed that it will not fail at that stage. So the user will not know straight away whether the ultimate action would be successful.

### Think differently
With a queue-based approach, your design thinking should be different. Rather than an immediate user feedback, you should provide a UX where the user will learn about the failed messages in the queue. This is done in the context of the message processing microservice, rather than the calling microservice.

### Manual intervention
A simple resolution in such cases will be to provide a list of failed messages to the user, with the ability to retry, or complete manually and dismiss the item. For example, let's consider a scenario where:

- you want the `e-shop` microservice to invoke a command named `SubmitOrder` in the `Orders` microservice.
- The context is where an online customer has added products to their basket, and is ready to complete the purchase.
- If using a Web Api approach, if the `SubmitOrder` fails, you would have to show an error to the customer to say *"sorry there is a problem"*. But then the customer will be lost!
- If using a message queue approach, the customer will not see an error, even when the `Orders` microservice fails to process the message.
- To ensure the order doesn't get lost, in the `Orders` microservice, a UI should be shown to the user (perhaps with email notifications) so it can be manually intervened. 
  - Maybe there is an error in the `Orders` application. Once fixed, the order message can be retried.
  - Maybe there is a data or logic issue, where e.g. the product is out of stock. Again, a corrective action can be taken by the user (either buy more stock, or have the customer service team call the buyer to discuss alternatives, etc.)

> With a queue-based architecture, the act of *command invocation* is simplified and guarded from the process of *exception resolution*. This brings resiliency to the former, and flexibility to the latter. Nevertheless, this needs a mindset shift in terms of the application or wireframe design.


## EventBusCommandMessage
The easiest way to implement a queue-based command integration is by using the `EventBusCommandMessage` class.
The process is very straight forward.

1. In the service that handles the command, create a subclass of EventBusCommandMessage.
1. Define the command arguments as instance properties or fields of the class.
1. Override the `Process()` method and write the handling logic.

For example:
```csharp
namespace BarService
{
    public class FooCommand : EventBusCommandMessage
    {
        public string Argument1;
        public int Argument2;
        
        public override async Task Process()
        {
             Console.WriteLine($"Command received. I will foo with {Argument1} and {Argument2}.");
             ...
        }
    }
}
```
To register the command, you need to add the following to your `Startup.cs` file:
```csharp
public override async Task OnStartUpAsync(IApplicationBuilder app)
{
    ...
    EventBus.Queue<BarService.FooCommand>().Subscribe();
}
```

## Queue Configuration
By convention, the queue url is expected in your `appSettings.json` file under the following key:
```json
...
"EventBus": {
    "Queues": {
        "BarService.FooCommand": {
            "Url": "https://sqs.xxx.amazonaws.com/.../BarService-FooCommand"
        }
},
```
The above queue configuration should be added to the config file of both the service that handles the command, as well as each service that invokes it.

## Invoking a command
To invoke a command in other services, all you need to do is to get a handle to the same queue and invoke the `Publish` command.
```csharp
await EventBus.Queue<BarService.FooCommand>().Publish(new BarService.FooCommand { ... });
```
Of course for the above to compile, you will need the command schema defined in the calling service as well. Note that the Process method will not be required, and also the base class should be just `EventBusMessage` rather than `EventBusCommandMessage`.

For the above example you can add the following class:
```csharp
namespace BarService
{
    public class FooCommand : EventBusMessage
    {
        public string Argument1;
        public int Argument2;
    }
}
```

## Generating a proxy
Of course with the above manual approach, you may end up in a situation where the class schemas across the handler and calling services go out of sync when changes occure. To simplify this process and also guarantee **consistency of the schema**, you can generate a client nuget package directly from the handler service and use that in the calling services. That way you won't have to copy the command message schema code.

To generate the nuget package, run the following command.
```
C:\> dotnet tool install -g generate-eventbus-command-proxy

C:\> generate-eventbus-command-proxy /assembly:C:\Projects\...\website.dll /command:FooCommand /out:C:\...\PrivatePackages
```
Or, if you want to directly publish the generated nuget package to a nuget server, instead of `/out:...` parameter add `/push:http://my-nuget-server.com/nuget /apiKey:...`

The generated proxy dll will generate the class, plus a method named `Publish()` for convinience. For example:
```csharp
namespace BarService
{
    public class FooCommand : EventBusMessage
    {
        public string Argument1 {get; set;}
        public int Argument2 {get; set;}
        
        public Task Publish () => EventBus.Queue<BarService.FooCommand>().Publish(this);
    }
}
```
So that you can simply invoke the command by writing the following in the services:
```csharp
await new BarService.FooCommand { ... }.Publish();
```
