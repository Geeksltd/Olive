# Logging

Olive relies on the built-in .NET Core Logging system. It's recommended that you [learn about that framework here](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1&tabs=aspnetcore2x) first.
Olive provides additional simplification features to the standard .NET Core Logging API.

## Logging data
To create a log message in your application you first need to obtain an `ILogger` instance using `Olive.Log.For(this)`. Or, if your code is in a static method, then `Olive.Log.For(typeof(MyClass))` or simply `Olive.Log.For<MyClass>()`.

Once you obtain the logger instance, you then invoke the appropriate method, depending on your desired log level. For example `Olive.Log.For(this).Error("Hello World")`.

## Log Levels

| Level | Description |
|-------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Critical | Logs that describe an unrecoverable application or system crash, or a catastrophic failure that requires immediate attention. |
| Debug | Logs that are used for interactive investigation during development. These logs should primarily contain information useful for debugging and have no long-term value. |
| Error | Logs that highlight when the current flow of execution is stopped due to a failure. These should indicate a failure in the current activity, not an application-wide failure. |
| Information | Logs that track the general flow of the application. These logs should have long-term value. |
| Trace | Logs that contain the most detailed messages. These messages may contain sensitive application data. These messages are disabled by default and should never be enabled in a production environment. |
| Warning | Logs that highlight an abnormal or unexpected event in the application flow, but do not otherwise cause the application execution to stop. |

Now let's log a *Hello world* to see how Olive records a log. Go to **Get** method **ValuesController** and add this code in the first line of the method. Feel free and use this method where ever you need to log something.

```csharp
Audit.Record("Log","Hello World");
```

### Passing object information

You can pass object information into the log. Also **Error** log type supports *Exceptions*.
Example:

```csharp
var userInput = new List<string>();
userInput.Add("BadData");
List<int> ints = null;
Log.Warning("User sent a bad data", userInput);
Log.Info("i'm going to throw application through an exception", this);
try
{
    ints.Add(3);
}
catch (Exception ex)
{
    Log.For(this).Error(ex);
}
```

Output:

![image](https://user-images.githubusercontent.com/22152065/37423630-98bf95dc-27d3-11e8-9e92-f26cf9f82641.png)

## Log providers
There are many standard and open source log providers. By default in Olive you will get:

- **Console provider**: Logs information into a console
- **Debug provider**: Logs information into debug window. Extreamly handy during development.
- **File provider**: Logs data into a text file.

### How to use providers

Everything is done in your **appsettings.json**. In your project, check out the sample `Log` section provided. You can learn more about [configuring this here](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1&tabs=aspnetcore2x#log-filtering).
