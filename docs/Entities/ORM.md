# Olive ORM (Object Relational Mapping)

Olive entities are basic C# objects. You can therefore use them with any ORM technology, such as *Entity Framework*. Though, they won't invoke the lifecycle events, and you will need to be careful in how you write your business logic related to object lifecycle.

Olive provides an ORM framework that is specifically designed for easier use, higher flexibility and better performance than Entity Framework or almost any other ORM technology. For example, in executing queries, it's on average twice as performant as Entity Framework.

## IDatabase
The `IDatabase` concept provides you with a facade, or an entry point, for all data operation requirements. Its default implementation class, named `Database`, will be sufficient for almost all applications. Though, you will be able to provide your own implementation if required.

It provides an extremely simple API to do all common data querying and manipulation scenarios including querying datasets, saving, deleting, bulk inserts, etc. It is designed to work with Olive based entities, i.e. your custom business classes that implement `IEntity`.

All of its operations are `async`, which ensures the highest throughput and performance in modern applications.

For example, to insert a record in the database, you will use a simple command such as:
```csharp
await Database.Save(new Customer { Name = "My customer" });
```

Or to fetch an object given its ID, you will use:
```csharp
var myCustomer = await Database.Get<Customer>(myId);
```

You will learn about all of its operations and features later in this guide.

### IDataProvider


The Olive ORM framework provides the following essential components.


### IDatabase

### IDatabaseQuery<T>
  


