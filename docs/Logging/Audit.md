# Olive.Audit

*Olive.Audit* helps you to log and record application events such as **Insert**,  **Update**, **Delete** or other application event. *Olive.Audit* saves the event records into database with **Olive.Audit.DatabaseLogger** provider.

## Logging events

You can log application events in *M# web apps* by using these methods:

```csharp
//Recording an update event
await Audit.LogUpdate(entity)

//Recording a detele event
await Audit.LogDelete(entity)

//Recording an insert event
await Audit.LogInsert(entity)

```

>**Note:** `entity` is an object of a class that impelimented `IEntity`

Also you can record uncategorized and custom events:

```csharp
await Audit.Log(@event, data)

//Also you can pass entity, userId and userip
```

### Customize Audit Service
To do so, you can simply extend the `Olive.Audit.DefaultAudit` or 
implement the `Olive.Audit.IAudit`.
Then you can inject the service to service collection in the `Startup.cs`
like the following code.
```csharp
public override void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<Olive.Audit.IAudit, Audit>();

    base.ConfigureServices(services);
}
```
> **Important** you should add it before calling the base `ConfigureServices`.

### Checkout recorded logs

**Olive.Audit.DatabaseLogger** records information in the database. If you want to see the recorded informatin, open your SSMS, go on your database tables and search for a table named **AuditEvents**. Query this table and see the recorded information.

![image](https://user-images.githubusercontent.com/22152065/37540926-092b877c-296e-11e8-9ecf-944597be8300.png)

>**Note:** We assume that you use *MSSQL Server* as your primary database. if you use any other RDBMS, go ahead and use related database management tools. The information is recorded on `AuditEvents` table.