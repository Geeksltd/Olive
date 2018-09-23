# Olive: Dev Commands
Olive provides a mechanism to invoke commands on a running application, to help with testing, debugging and diagnostics.

There are a number of such commands built into the Olive framework and several of its plugins, but you can create your own also.

When your application is running under the `Development` environment *(e.g. via LaunchSettings.json)*, these commands will become invocable.
But they are ignored if the application is running under any other environment such as `Staging` or `Production`.

## Creating your own DevCommand
To create a dev command, all you need to do is to create a class that implements `Olive.IDevCommand` and register it in the standard ASP.NET Dependency Injection framework.

The `IDevCommand` interface has the following members:

```csharp
// A command that can be sent to the application during development time.
public interface IDevCommand
{
    // Programmatic name of the command.
    string Name { get; }

    
    // A text or title for this command (optional). If set, it will be shown to the developer on the UI.
    string Title { get; }

    // Invokes the command.
    // It should return true if the command ends the http request processing and sends a response to the user.
    Task<bool> Run();

    // Determines whether this command is usable in the current context and configuration.
    bool IsEnabled();
}
```
