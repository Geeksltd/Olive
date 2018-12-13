# Data Replication

Typically, in microservices architecture, you will have services that need data from other services.
Let's say we have a `publisher service` that owns some data which is needed by a `consumer service`.
Of course, the publisher service can make its data available to the consumer service, via an API.

### Api approach: Performance and resiliency
The consumer processes may frequently need access to the publisher's data. If they make an API call every time they need it, it can be very slow.  But more importantly, what if the publisher service is unavailable? Well, the consumer service will also fail!
This defeats the purpose of using microservices architecture in the first place.

> To solve the performance problem, the consumer service can cache the data for a desired period of time.
A support for this is built-in to the Olive `ApiClient` framework. To solve the resiliency problem, the `ApiClient` framework in Olive, allows reusing the earlier snapshots of the cached data, even when the cache lifetime is passed.

### Api approach: Data recency
While the caching approach can somewhat solve the performance and resiliency problem, but it introduces another problem, which is data recency. In other words, a change in the publisher data will not be seen immediately by the consumer and so its cache will not be automatically updated.

> You can, of course, make a two way system where every change in the publisher data would notify the consumer to refresh its cache. 
But that will be complex to implement, unreliable *(what if the consumer service isn't available at that moment?)* and inefficient *(a single record change can invalidate a whole set of cached data unnecessarily)*.

---

## Better approach: Replication
To solve the problems mentioned above with an Api-based integration fo data, you can use the data replication approach.
In this approach, a read-only copy of the data will be created in the consumer service's database, and it's kept up-to-date via an event bus and queueing system, for a reliable, fast and efficient result.

In this approach, the `publisher service` will define an `Data Endpoint` for the `consumer service` to declaratively specify:
- What entity types' data can be made available to the consumer
- What fields
- What filter criteria (to limit the data to a subset if required)

### Example
The following example is defined in an imaginary publisher microservice called the `Customer Service` which holds customer data.
Let's say there is another microservice called `Order Service` which is responsible for managing customer orders.
In the orders service, we need access to the customer data, for example to associate orders to them.

To implement this, in the customer microserice project, we will create the following class:

```c#
namespace CustomerService
{    
    public class OrdersEndpoint : SourceEndpoint 
    {
        class Customer : ExpseedType<Domain.Customer>
        {
            protected override void Define()
            {
                Expose(x => x.Email);
                Expose(x => x.Name);
            }
        }
    }
}
```
The `Customer` class here provides a definition for a data table to expose. It inherits from `ExposedType<...>` which is a special class in Olive. The above code is saying *"expose the data from my local `Domain.Customer` type as a type, also called `Customer`. But only expose the `Email` and `Name` fields."*.

### One endpoint, multiple exposed data types
In the same endpoint, you can expose multiple data types. For each data type you need to define a **nested class** which inherits from`ExposedType<TDomain>`.

```c#
namespace CustomerService
{    
    public class OrdersEndpoint : SourceEndpoint 
    {
        class Customer : ExposedType<Domain.Customer>
        {
            protected override void Define()
            {
                Expose(x => x.Email);
                Expose(x => x.Name);
            }
        }
        
        public class CustomerAddress : ExposedType<Domain.CustomerAddress>
        {
            protected override void Define()
            {
                Expose(x => x.Line1);
                ...
            }
        }
    }
}
```
### One exposed type definition, multiple endpoints
It is strongly recommended that you do not share the same exposed type definition across multiple endpoints. For security, efficiency and ease of future changes, it's advisable to dedicate each exposed type definition to one endpoint, which is used by one consumer client service.

But, if you have a strong reason to do this, then:
- Instead of clraing the `ExposedType` subclass as a *nested class* of one endpoint class, define it as a whole class directly in the namespace.
- In each of the endpoints that you want to publish it, add a `[ExportData(typeof(...))]` attribute.
For example:

```c#
public class Customer : ExposedType<Domain.CustomerAddress>
{
   ...
}

[ExportData(typeof(Customer))]
public class OrdersEndpoint : SourceEndpoint { }

[ExportData(typeof(Customer))]
public class ShippingEndpoint : SourceEndpoint { }
```

### Exposing all fields implicitly
You should normally only expose the data fields needed by the endpoint consumer. As explained before, to specify the fields in an `ExposedType<T>` you will override the `Define()` method. However if your entity is basic or of a reference type nature (rather than transactional data) and you do not have security concerns, then you can use the following shortcut as an alternative to declaring each property directly:

```c#
public class Customer : NakedExposedType<Domain.CustomerAddress> { }
```

The `NakedExposedType<TDomain>` class will automatically expose all properties of the specified domain type.

### Exposing custom fields / mappings
You do not have to expose your data fields exactly as they are in the source type definition. For example, let's say you have multiple address fields in your Customer entity. But for a consumer service, you'd rather export the address as a single field.

To achieve that, you can define a custom exposed field, with a custom value expression. For example:

```c#
...
protected override void Define()
{
    Expose(x => x.Name);
    Expose("Address", x => new { x.AddressLine1, x.AddressLine2 + x.Town + x.Postcode }.Trim().ToString(", "));
}
```

---

## Generating a proxy
A utility named **generate-data-endpoint-proxy** (distributed as a nuget global tool) will be used to generate private nuget packages for the data endpoint, to be used by the `consumer service`. 

```batch
C:\> dotnet tool install -g generate-data-endpoint-proxy

C:\> generate-data-endpoint-proxy /assembly:"c:\...\website.dll" /dataEndpoint:OrdersEndpoint /out:"c:\temp\generated-packages\"
```

It will generate the following two nuget packages:

#### Package 1: CustomerService.OrdersEndpoint

This package will be referenced by the `Website` project in the consumer service (e.g. Orders microservice).
In the `Startup.cs` file to kick start the engine, call:

```c#
public override async Task OnStartUpAsync(IApplicationBuilder app)
{
    await base.OnStartUpAsync(app);
    await new CustomerService.OrdersEndpoint(typeof(CustomerService.Order).Subscribe();
}
```

When the `Subscribe()` method is called, it will do the following:
- If its local database table of *CustomerService.Customers* is empty, then it assumes that a full table fetch is required:
  - It invokes `EventBus.Purge(queueKey)` to clear any unprocessed messages on the sync queue.  
  - It invokes a Web Api on the `CustomerService` to fetch the full database as a clean starting point.
- It will then watch all changes made to the data on the publisher side (via an event bus queue) and keep its local copy of the data up-to-date.

#### Package 2: CustomerService.OrdersEndpoint.MSharp
This package will be referenced by the consumer service's `#Model` project to enable the necessary code generation.

Each subclass of `ExposedType<TDomain>` defined in the publisher service, represents one entity type in the consumer service, which is either a full or partial clone of the main `TDomain` entity type in the publisher service. In the above example:
- We have a `Domain.Customer` class in the publisher service (`Customer Service`) which has the full customer data, perhaps with 20 fields.
- In the consumer service (Orders) we need to have the `customer` concept for various programming activities. We may want to add business logic to it, or add associations to other entities, etc. But we only care about its Name and Email fields (and ID of course).
- For security, efficiency and simplicity, we want the Order service to only see a limited view of the main `Customer` entity.
- The `PeopleService.Customer` class which inherits from `ExposedType<Domain.Customer>` serves that purpose. It is in fact a remote representative of the customer concept with limited data in the consumer's world.
- In the consumer service (Orders) we will need that Customer concept to be present, giving us programmatic access, intellisense, data querying, etc. 

To make it all happen, we generate an M# nuget package which contains the definition of the `Customer` entity from the perspective of the consumer application. It's basically a DLL with a normal M# entity definition.

The following code is therefore generted by the **generate-data-endpoint-proxy** tool, from the `PeopleService.Customer` class.
```c#
using MSharp;
namespace PeopleService
{
    public class Customer : EntityType
    {
        public Customer()
        {
            Schema("PeopleService");
            String("Email");
            String("Name");
        }
    }
}
```
For all intents and purposes, this is a normal M# entity definition. (**TODO: Find a way to prevent save/delete operations on it**)
