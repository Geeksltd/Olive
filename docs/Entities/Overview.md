# Olive Entities
Almost every business application deals with business entities. 

An entity is some unit of data that can be classified and have stated relationships to other entities.
In terms of the database, an entity is a single person, place, or thing about which data can be stored.

Olive introduces some basic abstractions for business entities, in order to provide frequently needed functionality out of the box. These abstractions enable useful and often required functionality such as data storage, logging, sorting, replication, CSV conversions, etc.

## IEntity
The `IEntity` interface is the most basic abstraction, with the following structure.
```
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
    object GetId();
   
    // Creates a new object that is a copy of the current instance.
    IEntity Clone();
}
```
Most build-in Olive services use this abstraction to differentiate between entity types and other classes. If you have a custom class that you want to use with such services, you will need to implement this interface. In most cases, however, you will not need to explicitly implement it. Instead, you will use inheritence to derive your application-specific types from existing base entity implementations, explained below.

# Entity, Entity<T>, GuidEntity, IntEntity, StringEntity
  
Olive provides the `Entity` class. It is an abstract type that implements `IEntity`. You can see its implementation [here](https://github.com/Geeksltd/Olive/blob/master/Olive.Entities/Entity.cs).

In addition to implementing the `IEntity` members, it also provides additional functionality related to lifecycle management of entity objects.
