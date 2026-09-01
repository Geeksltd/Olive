# Olive.Audit.DatabaseLogger

## Overview
The `DatabaseLogger` class is responsible for logging audit events into a database. It implements the `IAuditLogger` interface and provides a mechanism to dynamically determine and instantiate audit event types.

## Features
- Supports dynamic resolution of IAuditEvent implementations.
- Logs audit events asynchronously into the database.
- Provides an extension method for easy integration with `IServiceCollection`.    

## Extension Method: `DatabaseLoggerExtensions`
The `DatabaseLoggerExtensions` class provides a convenient method to register the `DatabaseLogger` in the service collection.

### `AddDatabaseLogger(IServiceCollection services)`
 
- **Purpose**: Registers `DatabaseLogger` as a transient service implementing `IAuditLogger`.
- **Usage**:
```csharp
public override void ConfigureServices(IServiceCollection services)
{
	...
	services.AddDatabaseLogger();
	...
}
``` 

## Conclusion
The `DatabaseLogger` class provides a flexible and extensible solution for logging audit events into a database. Its dynamic event type resolution and seamless DI integration make it a powerful tool for auditing systems.