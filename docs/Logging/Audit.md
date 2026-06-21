# Olive.Audit

**Olive.Audit** provides structured logging for entity changes, ensuring that all insertions, updates, and deletions are efficiently recorded. This includes entity processing, data extraction, and database storage. The event records are saved into the database using the **Olive.Audit.DatabaseLogger** provider.

## Installation Steps

### 1. Install Required Packages
Install the following NuGet packages:
- `Olive.Audit`
- `Olive.Audit.DatabaseLogger`

### 2. Define the `AuditEvent` Entity
Add the `AuditEvent` entity to the **Domain**:

```csharp
using MSharp;

namespace Domain
{
    class AuditEvent : EntityType
    {
        public AuditEvent()
        {
            Implements("Olive.Audit.IAuditEvent");

            DateTime("Date").Mandatory().DefaultToNow();
            String("Event").Mandatory();
            String("Item Id");
            String("Item Type");
            String("Item Group");
            String("User Id");
            String("User IP");
            BigString("Item Data");            
        }
    }
}
```

### 3. Implement `ContextUserProvider`
Create a `ContextUserProvider` class in **Domain** and implement `IContextUserProvider`:

```csharp
public class ContextUserProvider : IContextUserProvider
{
    public ClaimsPrincipal GetUser()
    {        
        return Context.Current.User();    
    }
    public string GetUserIP()
    {         
        return Context.Current.Http().Connection.RemoteIpAddress?.ToString();      
    }
}
```

### 4. Register Dependencies

Add the following services in `Startup.cs`:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    ...         
    services.AddScoped<Olive.Audit.IContextUserProvider, ContextUserProvider>();
    services.AddDatabaseLogger();            
    ...            
}
```

### 5. Enable Automatic Data Logging
To enable automatic logging of entity operations (insert, update, delete), modify the `appSettings.json` file as follows:

```json
"Database": {
    ...
    "Audit": {
        "Insert": { "Action": true, "Data": true },
        "Update":  { "Action": true, "Data": true },
        "Delete": { "Action": true, "Data": true }
    }
}
``` 

### 6. Exclude or Include Specific Entities
- To **exclude** certain entities from being logged, set `EnableLogging(false)` in their M# definition.
- To **log only specific entities**, add the `[LogEvents(true)]` attribute to the entity’s partial class:

```csharp
using Olive.Entities;

namespace Domain
{
    [LogEvents(true)]
    public partial class EntityName
    {
    }
}
```

### 7. Customize the Audit Service
Extend `Olive.Audit.DefaultAudit` or implement `Olive.Audit.IAudit` to customize audit behavior. Example:

```csharp
namespace Domain
{
    public class Audit : Olive.Audit.DefaultAudit
    {
        public Audit(Microsoft.Extensions.Configuration.IConfiguration configuration) : base(configuration)
        {
        }
    }
}
```

Register this service in `Startup.cs` **before** calling `base.ConfigureServices`:

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<Olive.Audit.IAudit, Audit>();
    base.ConfigureServices(services);
}
```

## Viewing Recorded Logs

The `Olive.Audit.DatabaseLogger` stores records in the database. To view the recorded logs:

1. Open **SQL Server Management Studio (SSMS)**.
2. Navigate to your database tables.
3. Look for the table named **AuditEvents**.
4. Run a query to retrieve logged events.

> **Note:** If using a different RDBMS, check the `AuditEvents` table using the appropriate database management tools.

![image](https://user-images.githubusercontent.com/22152065/37540926-092b877c-296e-11e8-9ecf-944597be8300.png)

## Audit Log Rendering

`Olive.Audit` provides two extension methods (available from version **2.1.113** onwards) for rendering logs in HTML:

- `NewChangesToHtml()`: Converts new changes (Insert/Update) into HTML.
- `OldChangesToHtml()`: Converts previous data (Delete/Update) into HTML.

### Styling Audit Logs
These SCSS classes enable customization of audit log records:

```scss
.audit-log {
    // Base styles

    &.audit-log-old {
        // Styles for old changes
    }

    &.audit-log-new {
        // Styles for new changes
    }

    .audit-log-label {
        // Styles for labels
    }

    .audit-log-property {
        // Styles for property values

        &.audit-log-property-bool {
            // Styles for boolean properties
        }

        &.audit-log-property-entity {
            // Styles for associated entity values
        }
    }
}
```

## Summary

**Olive.Audit** efficiently tracks entity changes, ensuring data integrity and accountability. It provides:
- Structured logging of entity modifications.
- Seamless database integration.
- Flexible configuration via `appSettings.json`.
- Easy customization via dependency injection.

This makes it a robust solution for audit logging in M# web applications.