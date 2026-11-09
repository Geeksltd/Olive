# Olive.Aws.LambdaFunction

## Overview
The `Startup` and `Function<TStartup>` classes provide a structured way to configure and run AWS Lambda functions using .NET and Microsoft.Extensions.Hosting. These classes handle configuration, dependency injection, logging, and environment setup.

## Configuration
Ensure the following configurations are set in your application:
- **Aws:ServiceUrl** - The base service URL for AWS services.
- **Logging** - Logging configuration section for different environments.

## Installation
To integrate this package into your project, install the necessary dependencies:
```sh
Install-Package Microsoft.Extensions.Hosting
Install-Package Amazon.Lambda.Core
Install-Package Microsoft.Extensions.Logging
```

## Class: `Startup`

### `ConfigureConfiguration`
```csharp
public virtual void ConfigureConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
```
- **Summary**: Configures the application configuration using `HostBuilderContext`.
- **Usage**:
```csharp
public override void ConfigureConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
{
    base.ConfigureConfiguration(context, builder);
}
```

### `ConfigureServices`
```csharp
public virtual void ConfigureServices(IServiceCollection services)
```
- **Summary**: Configures dependency injection and merges environment variables into the configuration.
- **Usage**:
```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    services.AddSingleton<IMyService, MyService>();
}
```

### `ConfigureLogging`
```csharp
public virtual void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
```
- **Summary**: Configures logging based on the environment.
- **Notes**:
  - In **development mode**, logs are sent to the console.
  - In **production**, AWS logging provider is used.
- **Usage**:
```csharp
public override void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
{
    base.ConfigureLogging(context, builder);
    builder.AddConsole();
}
```

### `ConfigureHost`
```csharp
public virtual void ConfigureHost(IHostBuilder builder)
```
- **Summary**: Custom host configurations can be added in derived classes.
- **Usage**:
```csharp
public override void ConfigureHost(IHostBuilder builder)
{
    builder.ConfigureServices(services =>
    {
        services.AddSingleton<IMyCustomService, MyCustomService>();
    });
}
```

## Class: `Function<TStartup>`

### `Function Constructor`
```csharp
protected Function() : this(null)
protected Function(string[] args)
```
- **Summary**: Initializes a new instance of the function and configures the host.
 
### `Init`
```csharp
protected virtual void Init(IHostBuilder builder)
```
- **Summary**: Sets up configuration, services, and logging.
- **Usage**:
```csharp
protected override void Init(IHostBuilder builder)
{
    base.Init(builder);
    builder.ConfigureServices(services =>
    {
        services.AddSingleton<ISomeDependency, SomeDependency>();
    });
}
```

### `HostCreated`
```csharp
protected virtual void HostCreated(IHost host)
```
- **Summary**: Called after the host is created. Initializes context and logging.
- **Usage**:
```csharp
protected override void HostCreated(IHost host)
{
    base.HostCreated(host);
    Console.WriteLine("Lambda function initialized.");
}
```

### `LocalExecute`
```csharp
protected static Task LocalExecute<TFunction>(string[] args)
    where TFunction : Function<TStartup>, new()
```
- **Summary**: Allows local execution of the Lambda function. 

### `ExecuteAsync`
```csharp
public abstract Task ExecuteAsync(ILambdaContext context)
```
- **Summary**: Abstract method that must be implemented in derived classes to handle AWS Lambda execution.
- **Usage**:
```csharp
public override async Task ExecuteAsync(ILambdaContext context)
{
    Console.WriteLine("Lambda function executed.");
}
```

## Usage Example
```csharp
public class MyLambdaStartup : Startup
{
    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        //services.AddSingleton<IMyService, MyService>();
    }
}

public class MyLambdaFunction : Function<MyLambdaStartup>
{
    public override async Task ExecuteAsync(ILambdaContext context)
    {
        Console.WriteLine("Executing Lambda Function");
    }
}
```

## Conclusion
The `Startup` and `Function<TStartup>` classes provide a robust foundation for building AWS Lambda functions with dependency injection, configuration management, and logging. These classes ensure a scalable and maintainable architecture.
