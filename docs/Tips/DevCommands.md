# Olive: Dev Commands
Olive provides a mechanism to invoke commands on a running application, to help with testing, debugging and diagnostics.

There are a number of such commands built into the Olive framework and several of its plugins, but you can create your own also.

When your application is running under the `Development` environment *(e.g. via LaunchSettings.json)*, these commands will become invocable.
But they are ignored if the application is running under any other environment such as `Staging` or `Production`.

## Creating a Dev Command
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

    // Determines whether this command is usable in the current context and configuration.
    bool IsEnabled();
     
    // Invokes the command. After the command execution, if it returns null or empty,
    // the user will be redirected to the http url referrer, or the root of the application.
    // Otherwise, the returned string value will be rendered in the http response.     
    Task<string> Run();    
}
```

### Example

The following example shows a simple command implementation to invoke `Database.Refresh()`

```csharp
class DatabaseClearCacheDevCommand : IDevCommand
{
    IDatabase Database;
    public DatabaseClearCacheDevCommand(IDatabase database) => Database = database;
    
    public string Name => "db-clear-cache";
    public string Title => "Clear DB cache";
    public bool IsEnabled() => true;

    public async Task<string> Run()
    {
        await Database.Refresh();
        return null;
    }
}
```
In order for this command to be recognised, it should be registered as a service. For example you can add the following in your `Startup.cs` file under `ConfigureServices`:
```cshrap
services.AddSingleton<IDevCommand, ClearApiCacheDevCommand>();
```
However, to improve the application code's readability, it's better to define the registration as an extention method as demonstrated below:
```csharp
public static class DatabaseClearCacheExtensions
{
    public static DevCommandsOptions AddClearDatabaseCache(this DevCommandsOptions @this)
    {
        @this.Services.AddSingleton<IDevCommand, ClearApiCacheDevCommand>();
        return @this;
    }
}
```
That way, the end user's application can register your command along with all other commands, similar to:
```
if (Environment.IsDevelopment())
    services.AddDevCommands(x => x.AddCommandX().AddCommandY()...AddClearDatabaseCache());
```


## Invoking a Dev Command
To invoke a dev command a http request should be sent to the application with the url of `/cmd/{Name}`.

If the Dev Command implementation provides a non-empty value for the `Title` property, then a UI will be generated to invoke this command on the web pages. Basically, a link will be added for every such command currently registered in the application into a box called the **DevCommandsWidget**.

To enable the UI in your application, add `@Html.DevCommandsWidget()` right before `</main>` in your `Views/Layouts/***.cshtml` files.


## Core built-in commands
| Command     | Notes |
| ------------- | ----- |
| `db-clear-cache` | Simply calls `Database.Refresh()`. |
| `api-clear-cache` | A part of `Olive.ApiClient` plugin. When invoked, it clears all cached Api call responses. |
| `outbox`  | A part of `Olive.Email` plugin. When invoked, it will render a table with all generated emails message records in the database. It's useful in automated UI tests, to verify whether the application generated notification emails are working correctly. |
| `scheduled-tasks` |  A part of `Olive.Hangfire` plugin. Renders the status of all currently running scheduled tasks. |
| `db-get-changes` | Renders all non-query sql commands submitted to the database since the application started running. |
| `run-db-changes` | Executes a set of sql commands provided in the http request form value of `Data`. |
| `db-profile-start` | Starts measureing all sql query commands submitted to the database. |
| `db-profile-snapshot` | Generates a snapshot report of the profiling data so far, but keeps profiling. |
| `db-profile-stop` | Generates a snapshot report of the profiling data and stops profiling. |
| `db-restart` | Recreates the database from the sql script files in the `DB` folder. |
| `local-date?date=22/10/2020` | Overrides the application's local date to the specified value. It will affect `LocalTime.Now` rather than `DateTime.Now`. |
| `local-date?time=10:22:00` | Overrides the application's local time to the specified value.  |
| `local-date?date=now` | Resets the application's local time to the real value. |
| `test-context?name=MyTestName` | Sets the currently running UI Test name, which is used in `PredictableGuidGenerator` |
