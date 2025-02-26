# Olive.Aws.Mvc

## Overview
The `App<TStartup>` and `Startup` classes provide a structured way to configure and run AWS Lambda functions using ASP.NET Core. These classes handle configuration, dependency injection, logging, and request culture management.

## Configuration
Ensure the following configurations are set in your application:
- **Logging** - Configuration section for logging setup.
- **Aws:ServiceUrl** - The base service URL for AWS services.
- **Aws:Secrets:Id** - The AWS Secrets Manager ID for data protection.

## Installation
To integrate this package into your project, install the necessary dependencies:
```sh
Install-Package Microsoft.AspNetCore.Hosting
Install-Package Amazon.Lambda.AspNetCoreServer
Install-Package Amazon.Lambda.Logging.AspNetCore
```

## Class: `App<TStartup>`

### `Init`
```csharp
protected override void Init(IWebHostBuilder builder)
```
- **Summary**: Initializes the web host builder with the specified `TStartup` and configures logging.
- **Usage**:
```csharp
protected override void Init(IWebHostBuilder builder)
{
    base.Init(builder);
    builder.ConfigureLogging(logging => logging.AddConsole());
}
```

### `LocalRun<TApp>(string[] args)`
```csharp
protected static void LocalRun<TApp>(string[] args) where TApp : App<TStartup>, new()
```
- **Summary**: Runs the application locally for testing purposes.
 
### `LocalRun<TApp>(TApp app, string[] args)`
```csharp
protected static void LocalRun<TApp>(TApp app, string[] args) where TApp : App<TStartup>
```
- **Summary**: Runs the specified instance of the application locally.
 
### `ConfigureLogging`
```csharp
protected virtual void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder logging)
```
- **Summary**: Configures logging for the application.
- **Notes**:
  - In **development mode**, logs are sent to the console.
  - In **production**, AWS Lambda logging is enabled with additional details.
- **Usage**:
```csharp
protected override void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder logging)
{
    base.ConfigureLogging(context, logging);
}
```

## Class: `Startup`

### Constructor
```csharp
protected Startup(IWebHostEnvironment env, IConfiguration config)
```
- **Summary**: Initializes the startup class and sets the default culture.
- **Usage**:
```csharp
public MyStartup(IWebHostEnvironment env, IConfiguration config) : base(env, config) { }
```

### `ConfigureServices`
```csharp
public override void ConfigureServices(IServiceCollection services)
```
- **Summary**: Configures dependency injection and enables CORS.
- **Notes**:
  - Enables CORS policy named `AllowOrigin`.
  - If running in **production**, configures AWS data protection services.
- **Usage**:
```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services); 
}
```

### `ConfigureMvc`
```csharp
protected override void ConfigureMvc(IMvcBuilder mvc)
```
- **Summary**: Configures MVC and Razor Pages options.
- **Usage**:
```csharp
protected override void ConfigureMvc(IMvcBuilder mvc)
{
    base.ConfigureMvc(mvc); 
}
```

### `ConfigureRazorPagesOptions`
```csharp
protected virtual void ConfigureRazorPagesOptions(Microsoft.AspNetCore.Mvc.RazorPages.RazorPagesOptions options)
```
- **Summary**: Allows customization of Razor Pages settings.
- **Usage**:
```csharp
protected override void ConfigureRazorPagesOptions(RazorPagesOptions options)
{
     
}
```

### `GetRequestCulture`
```csharp
protected override CultureInfo GetRequestCulture()
```
- **Summary**: Sets the default request culture to `en-GB`.
- **Usage**:
```csharp
protected override CultureInfo GetRequestCulture()
{
    return new CultureInfo("fr-FR");
}
```

## Full Example
```csharp
public class MyStartup : Startup
{
    public MyStartup(IWebHostEnvironment env, IConfiguration config) : base(env, config) { }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        //services.AddSingleton<IMyService, MyService>();
    }
}

public class MyLambdaApp : App<MyStartup>
{
} 
```

## Conclusion
The `App<TStartup>` and `Startup` classes provide a robust framework for building AWS Lambda applications using ASP.NET Core. These classes ensure proper logging, dependency injection, request culture handling, and allow local execution for easy testing and debugging.
