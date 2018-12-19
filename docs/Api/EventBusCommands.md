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
