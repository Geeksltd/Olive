# Olive.Console

## Overview
Olive is used by default in M# project templates. You can also add Olive nuget packages to any other project to use the extension methods.

Some of the Olive features however, will need the core Olive context and services to be set up and available before you can use them.
This includes EventBus and various other specific Olive services.

The `Application` class is a host container designed for console applications that leverage Olive services. It provides an entry point for initializing and running console-based applications with dependency injection, logging, and configuration management.

## Methods

### `Start<TStartup>(string[] args, Action<HostBuilder> configure = null)`
- **Description**: Initializes and runs the console application asynchronously.
- **Parameters**:
  - `args`: Command-line arguments passed to the application.
  - `configure`: An optional configuration action for the `HostBuilder`.
- **Returns**: `Task` representing the asynchronous operation.
- **Usage**:
  ```csharp
  await Application.Start<MyStartup>(args);
  ``` 

### `StartUnitTest<TStartup>(Action<HostBuilder> configure = null)`
- **Description**: Initializes the application in unit test mode.
- **Parameters**:
  - `configure`: An optional configuration action for `HostBuilder`.
- **Usage**:
  ```csharp
  Application.StartUnitTest<MyTestStartup>();
  ```

---

# Startup Class Documentation

## Overview
The `Startup` class provides a base implementation for configuring services and running console applications in an Olive-based system.

## Methods

### `ConfigureServices(IServiceCollection services)`
- **Description**: Configures required services for the application.
- **Usage**:
  ```csharp
  public override void ConfigureServices(IServiceCollection services) {
      services.AddLogging();
  }
  ```  

---

## Example Usage
To activate the Olive context in a console app add a nuget reference to `Olive`, `Olive.Console` and any other required Olive services. Then replace your Program startup file with the following:

```csharp
class Program
{
    static Task Main(string[] args) => Olive.Console.Application.Start<Startup>(args);
}

class Startup : Olive.Console.Startup
{
    public Startup(IConfiguration config) : base(config)
    {
        // Using AWS services?
        // config.LoadAwsDevIdentity("...", "...", Amazon.RegionEndpoint.EUWest1, loadSecrets: false);
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        // ... any specific configurations will go here.
        
        services.AddDefaultAudit();        

        // Using AWS stuff??
        services.AddAwsEventBus();
        services.AddAWSService<Amazon.S3.IAmazonS3>();

        // Using database?
        services.AddDatabase();
        services.AddDataAccess(x => x.SqlServer());
    }
    
    static IDatabase Database => Context.Current.Database();

    protected override Task Run()
    {
        Database.ConfigDataAccess().Configure();
        
        Console.WriteLine("Hello!");
        return Task.CompletedTask;
    }
}
```
