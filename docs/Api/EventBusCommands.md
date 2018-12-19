# EventBus Commands

Often a microservice should invoke a command in another microservice.

This is somewhat similar to sending POST, PUT or DELETE http messages to APIs, without expecting a return value.
However, the benefit of using a queue-based approach is that it's more **reliable** and **scalable**.

## EventBusCommandMessage
The easiest way to implement a queue-based command integration is by using the `EventBusCommandMessage` class.
The process is very straight forward.

1. In the service that handles the command, create a subclass of EventBusCommandMessage.
1. Define the command arguments as instance properties or fields of the class.
1. Override the `Process()` method and write the handling logic.

For example:
```c#
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
```c#
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
```c#
await EventBus.Queue<BarService.FooCommand>().Publish(new BarService.FooCommand { ... });
```
Of course for the above to compile, you will need the command schema defined in the calling service as well. Note that the Process method will not be required. For the above example you can add the following class:
```c#
namespace BarService
{
    public class FooCommand : EventBusCommandMessage
    {
        public string Argument1;
        public int Argument2;
    }
}
```
