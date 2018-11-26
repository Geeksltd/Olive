# Data Replication

Typically, in microservices architecture, you will have services that need data from other services.
Let's say we have a `publisher service` that owns some data which is needed by a `consumer service`.

## Background: Problems with Api integration
Of course, the publisher service can make its data available to the consumer service, via an API.

### Performance and resiliency
The consumer processes may frequently need access to the publisher's data. If they make an API call every time they need it, it can be very slow. 
But more importantly, what if the publisher service is unavailable? Well, the consumer service will also fail!
This defeats the purpose of using microservices architecture in the first place.

To solve the performance problem, the consumer service can cache the data for a desired period of time.
A support for this is built-in to the Olive `ApiClient` framework.

To solve the resiliency problem, the `ApiClient` framework in Olive, allows reusing the earlier snapshots of the cached data, even when the cache lifetime is passed.

### Data recency
While the caching approach can somewhat solve the performance and resiliency problem, but it introduces another problem, which is data recency.
In other words, a change in the publisher data will not be seen immediately by the consumer and so its cache will not be automatically updated.

You can, of course, make a two way system where every change in the publisher data would notify the consumer to refresh its cache. 
But that will be: 

- Complex to implement
- Unreliable (what if the consumer service isn't available at that moment?)
- Inefficient (a single record change can invalidate a whole set of cached data unnecessarily)

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
    using Olive.Entities;

    public class Customer : ReplicatedData<Domain.Customer>
    {
        protected override void Define()
        {
            Export(x => x.Email);
            Export(x => x.Name);
        }
    }
    
    [ExportData(typeof(Customer))]
    public class OrdersEndPoint : DataReplicationEndPoint { }
}
```
The `Customer` class here provides a definition for a data table to expose. It inherits from `ReplicatedData<...>` which is a special class in Olive. The above code is saying *"Export the data from my local `Domain.Customer` type into a type, also called `Customer`. But only export the `Email` and `Name` fields."*.

#### Exporting multiple data tables
In the same endpoint, you can export multiple data types. For each data type you need to define a sub-class of `ReplicatedData` and then add that to the endpoint using another `ExportData` attribute. For example:

```c#
public class CustomerAddress : ReplicatedData<Domain.CustomerAddress>
{
    protected override void Define()
    {
        Export(x => x.Line1);
        ...
    }
}

[ExportData(typeof(Customer))]
[ExportData(typeof(CustomerAddress))]
public class OrdersEndPoint : DataReplicationEndPoint { }
```

Here, a data end point is created, called `OrdersEndPoint`. Using an `ExportData` attribute, it's linking  The end point can export data for multiple data replication definitions. In the above example

### Generating a proxy
A utility named **generate-data-endpoint-proxy** (distributed as a nuget global tool) will be used to generate private nuget packages for the data endpoint, to be used by the `consumer service`. 

```batch
C:\> dotnet tool install -g generate-data-endpoint-proxy
C:\> generate-data-endpoint-proxy /assembly:"c:\...\website.dll" /dataEndPoint:OrdersEndPoint /out:"c:\temp\generated-packages\"
```

It will generate the following two nuget packages.

#### CustomerService.OrdersEndPoint

This package will have the following generated class:
```c#
namespace CustomerService
{
   public class OrdersEndPoint : DataReplicationEndPointConsumer   
   {   
       static Type CustomerType => Type.GetType("CustomerService.Customer");
       static Type CustomerAddressType => Type.GetType("CustomerService.CustomerAddress");
   
       /// <summary> Clears all messages from the queue of customer data. It will then 
       /// fetch the current data directly from the CustomerService. </summary>
       public static Task RefreshCustomerData() => RefreshData(CustomerType);
              
       /// <summary> Clears all messages from the queue of customer address data. It will then 
       /// fetch the current data directly from the CustomerService. </summary>
       public static async Task RefreshCustomerAddressData() => RefreshData(CustomerAddressType);
       
       /// <summary> It will start listening to queue messages to keep the local database up to date
       /// with the changes in the Customer service. But before it starts that, if the local table 
       /// is empty, it will refresh the full data. </summary>
       public static async Task Subscribe()
       {
           await Subscribe(CustomerType);
           await Subscribe(CustomerAddressType);
       }
   }
}
```

To see how the above code works, look into [this class](https://github.com/Geeksltd/Olive/blob/master/Olive.Entities.Data.Replication/DataReplicationEndPointConsumer.cs).

This package will be referenced by the `Website` project in the consumer service (e.g. Orders microservice). In the `Startup.cs` file to kick start the engine, it should call:

```c#
public override async Task OnStartUpAsync(IApplicationBuilder app)
{
    await base.OnStartUpAsync(app);
    CustomerService.OrderssEndPoint.Subscribe();
}
```

When the `Subscribe()` method is called, it will do the following:
- If its local database table of *CustomerService.Customers* is empty, then it assumes that a full table fetch is required:
  - It invokes `EventBus.Purge(queueKey)` to clear any unprocessed messages on the sync queue.  
  - It invokes a Web Api on the `CustomerService` to fetch the full database as a clean starting point.
- It will then watch all changes made to the data on the publisher side (via an event bus queue) and keep its local copy of the data up-to-date.

#### CustomerService.OrdersEndPoint.MSharp
This package will be referenced by the consumer service's `#Model` project to enable the necessary code generation.

Each subclass of `ReplicatedData<TDomain>` defined in the publisher service, represents one entity type in the consumer service, which is either a full or partial clone of the main `TDomain` entity type in the publisher service. In the above example:
- We have a `Domain.Customer` class in the publisher service (`Customer Service`) which has the full customer data, perhaps with 20 fields.
- In the consumer service (Orders) we need to have the `customer` concept for various programming activities. We may want to add business logic to it, or add associations to other entities, etc. But we only care about its Name and Email fields (and ID of course).
- For security, efficiency and simplicity, we want the Order service to only see a limited view of the main `Customer` entity.
- The `PeopleService.Customer` class which inherits from `ReplicatedData<Domain.Customer>` serves that purpose. It is in fact a remote representative of the customer concept with limited data in the consumer's world.
- In the consumer service (Orders) we will need that Customer concept to be present, giving us programmatic access, intellisense, data querying, etc. 
