# Entity: Database lifecycle events
The `Entity` class provides **object lifecycle** methods and events that you can hook into for implementing custom business logic in your applications.

## Saving an entity in the database
When you save an entity in the database, whether for insert or update operation, the following events will be invoked automatically by the framework. They run in the following order:

### OnValidating()
This is invoked automatically as the first step when you try to save an entity in the database using the `Database.Save()` command. This executes before calling the `Validate()` method. You should not use it to implement your validation logic. Instead, use this to do any last-minute object modifications, such as initializing complex values.

For example:
```csharp
class Customer : GuidEntity
{
    public string OfficialName {get; set;}
    public string FriendlyName {get; set;}

    protected override async Task OnValidating()
    {
        await base.OnValidating();
        
        if (FriendlyName.IsEmpty() && OfficialName.HasValue())
           FriendlyName = OfficialName.TrimEnd(" Ltd", caseSensitive: false);
    }
}
```
        
### Validate()
This method is invoked by the framework when saving an entity, right after `OnValidating()` call is completed.
You should override this method in your business entity types in order to provide custom validation logic.
It expects a `ValidationException` to be thrown in case of an invalid state. Should that be the case, the saving operation will be terminated.

For example:
```csharp
class Customer : GuidEntity
{
    ...

    protected override async Task Validate()
    {
        await base.Validate();
        
        if (FriendlyName.IsEmpty() && OfficialName.HasValue())
           throw new ValidationException("When official name is specified, friendly name must also be specified.");
    }
}
```

### OnSaving()
This event is raised just before an entity is saved in the data repository. It is invoked right after a successful call to `Validate()`.
You can override this method to implement custom business logic in some rare scenarios. 

> Note: You should not normally use this to amend the state of the object, because in that case the validation logic will not be able to catch an error in your modifications. In such cases, use `OnValidating()` instead.

This method contains an argument of type `CancelEventArgs` which allows you to cancel the save operation. For example:
```csharp
class Customer : GuidEntity
{
    ...

    protected override async Task OnSaving(CancelEventArgs e)
    {
        await base.OnSaving(e);
        
        if (/*some scenario*/)
           e.Cancel = true;
    }
}
```

### OnSaved()
This event is raised after this instance is saved in the database. You can use it for simple workflows. 
It takes in an argument of type `SaveEventArgs` with a property named `Mode` which is an enum with the options of `Insert` and `Update`.

For example:
```csharp
class Customer : GuidEntity
{
    ...

    protected override async Task OnSaved(SaveEventArgs e)
    {
        await base.OnSaved(e);
        
        if (e.Mode == SaveMode.Insert)
        {
            // A new customer is just inserted. Notify the customer service team:
            Notifications.ReportRegistration(this);
        }
    }
}
```
### IsNew property
The base `Entity` class provides a property named `IsNew`. When a new object is instantiated in memory, it is set to `True` by default. 
However, when you load an entity from the database, the value will be set to `False` by the framework.

When writing custom business logic, by querying the value of `IsNew` you can determine whether the object is just created, or is loaded from the database. This value is available to you in all lifecycle event methods.

> The value of `IsNew` is also automatically set to `False` when an entity is saved in the database. This means that you should be very careful when writing lifecycle business logic in the `OnSaved()` event, because in that context, the value of `IsNew` is always false. You should instead use `e.Mode` to differentiate between insert and update scenarios.

## Loading an object from database
When you fetch an entity from the database, the framework will create a new instance of your entity type (e.g. `Customer`) and then load the data into it.

It will then immediately invoke the `OnLoaded()` event method on it, where you can add your own business logic.
In the following example, we use a mix of the `OnLoaded()` and `OnSaved()` events to track changes to the customer's address to notify them of the customer department team.

```csharp
class Customer : GuidEntity
{
    ...
    public string Address {get; set;}
    
    private string PreviousAddress;
    
    protected override async Task OnLoaded()
    {
        await base.OnLoaded(e);
        PreviousAddress = Address;
    }

    protected override async Task OnSaved(SaveEventArgs e)
    {
        await base.OnSaved(e);
        
        if (e.Mode == SaveMode.Update && Address != PreviousAddress)
        {
            Notifications.ReportCustomerRelocation(this);
        }
    }
}
```
In the above example, we hook into the `OnLoaded()` event to copy of the `Address` value as loaded from the database, into a local field named `PreviousAddress`. This is then compared in the `OnSaved()` event against the new and possibly modified value of Address.

## Deleting an entity
When you attempt to delete an entity from the database, the following events will execute in order:

### OnDeleting()
This method is called by the framework just before a delete command is sent to the database. Just like `OnSaving()`, this method provides an argument of type `CancelEventArgs`. You can use this method to prevent the delete operation in some rare scenarios.

When you override this method, you can write custom business logic to check for certain conditions and optionally cancel the delete operation by setting the `Cancelled` property to `true`.

```csharp
class Customer : GuidEntity
{
    ...

    protected override async Task OnDeleting(CancelEventArgs e)
    {
        await base.OnDeleting(e);
        
        if (/*some scenario*/)
           e.Cancel = true; // This will stop the delete operation.
    }
}
```

### OnDeleted()
This method is called by the framework just after a delete command is executed in the database. When you override this method, you can write custom business logic to execute after an object has been deleted.

```csharp
class Customer : GuidEntity
{
    ...

    protected override async Task OnDeleted()
    {
        await base.OnDeleted();        
        Notifications.ReportCustomerDeletion(this);
    }
}
```
