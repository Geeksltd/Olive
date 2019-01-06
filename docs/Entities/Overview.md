# Olive Entities
Almost every business application deals with business entities. 

An entity is some unit of data that can be classified and have stated relationships to other entities.
In terms of the database, an entity is a single person, place, or thing about which data can be stored.

Olive introduces some basic abstractions for business entities, in order to provide frequently needed functionality out of the box. These abstractions enable useful and often required functionality such as data storage, logging, sorting, replication, CSV conversions, etc.

## IEntity
The `IEntity` interface is the most basic abstraction, with the following structure.
```csharp
interface IEntity : IComparable
{
    // Determines whether this object has just been instantiated as a new object, or represent an already persisted instance.
    // When you instantiate a new entity type, e.g. "new Customer()", this will be true. 
    // When you fetch an existing entity record from the database, this will be false.
    // In your business logic code, you can often query its value to determine the state of the object.
    bool IsNew { get; }
   
    // Validates this instance and throws ValidationException if necessary.
    Task Validate();
   
    // Gets the id of this entity.
    // Depending on the implementation, this can be Guid, string, int or any other simple type.
    object GetId();
   
    // Creates a new object that is a copy of the current instance.
    IEntity Clone();
}
```
Most build-in Olive services use this abstraction to differentiate between entity types and other classes. If you have a custom class that you want to use with such services, you will need to implement this interface. In most cases, however, you will not need to explicitly implement it. Instead, you will use inheritence to derive your application-specific types from existing base entity implementations, explained below.

## Entity
  
Olive provides the `Entity` class. It is an abstract type that implements `IEntity`. You can see its implementation [here](https://github.com/Geeksltd/Olive/blob/master/Olive.Entities/Entity.cs). In addition to implementing the `IEntity` members, it also provides additional functionality related to lifecycle management of entity objects. You will learn about them later in this guide.

## Entity<T>
The `Entity<T>` class, which inherits from `Entity`, provides a version of entity concept whose *primary key* type is of the specified type of `T`, its generic argument. It provides a public property named `ID` which represents the primary key of the entity. 

The value of the ID is usually assign upon creation of the object, or its initial persistence in the database. Regardless of when it is set, usually it never changes later on. This allows you to uniquely identify an entity (record) in the system. For example, you can pass the ID in a http url as query string to identify the intended entity to the receipient page.

### GuidEntity
The `GuidEntity` class, for instance, inherits from `Entity<Guid>`. It is the most common base type for business objects as it almost guarantees uniqueness of object IDs as soon as one is instantiated in memory. It does not rely on a central orchestrator for ensuring uniqueness of IDs. For that reason, it's the easiest one to use and is the most flexible base class for normal business entity types.

Your application-specific business entity types often inherit from that to gain benefit of many built-in Olive features. For example:
```csharp
public class Customer : GuidEntity
{
}

...
void SomeMethodSomewhere()
{
    var customer = new Customer();
    Guid id = customer.ID; // This gives you a randomly generated unique Guid value.
}
```

### IntEntity and StringEntity
Just like `GuidEntity`, these classes inherit from `Entity<int>` and `Entity<string>` subsequently.
If you need your custom business entities to have their primary key be of type `int` or `string`, you can use these instead of `GuidEntity`. You should avoid using these unless you know exactly why you need them.

`Guid` based entities are favarourable in most scenarios. Their only downside is a small performance hit when used in sorting or comparisons in extremely high quantities (hundreds of thousands of records). In such scenarios, or in other rare cases, `int` or `string` based entities can be used.

## Database lifecycle events
The `Entity` class provides **object lifecycle** methods and events that you can hook into for implementing custom business logic in your applications.

For example, it provides the following methods which are invoked automatically by the framework when you try to *Save an entity* to the database. They run in the following order:

#### OnValidating()
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
        
#### Validate()
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

#### OnSaving()
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

#### OnSaved()
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


, `OnDeleting()`, `OnDeleted()`. To implement custom business logic related to the lifecycle events of a