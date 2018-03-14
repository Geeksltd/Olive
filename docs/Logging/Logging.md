# Logging

Olive framework provides fluent methods to log application events. Logging features exsist natively in [Olive nuget package](https://www.nuget.org/packages/Olive/).

## installation

Create a WebAPI ASP.NET core application then open up NuGet package manager and add [Olive nuget package](https://www.nuget.org/packages/Olive/) to your project.

```ps
Install-Package Olive
```

Now move to **appsettings.json** then add this configuration into it:

```json
"Log": {
    "Console": true
  }
```

Make sure that your ASP.NET core project is running directly not through IIS express. Otherwise you might not see a console output.

![image](https://user-images.githubusercontent.com/22152065/37421052-986097e0-27cd-11e8-8a4c-d0bd14d2b5e8.png)

## Logging data

Now let's log a *Hello world* to see how Olive records a log. Go to **Get** method **ValuesController** and add this code in the first line of the method. Feel free and use this method where ever you need to log something.

```csharp
Log.Record("Log","Hello World");
```

Now hit **F5** and run the project. Now you should see the output in the console:

![image](https://user-images.githubusercontent.com/22152065/37421584-e633502e-27ce-11e8-8838-a4c9ae993bba.png)

### Specific log types

Currently Olive supports these kinds of logs:

- **Record**: it's a general type of log
- **Audit**
- **Info**
- **Debug**
- **Warning**
- **Error**

You can use any of them according to your use case.

### Passing information into log

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
    Log.Error(ex);
}
```

Output:

![image](https://user-images.githubusercontent.com/22152065/37423630-98bf95dc-27d3-11e8-9e92-f26cf9f82641.png)
