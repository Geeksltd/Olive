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

```cshrap
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
A utility named **generate-data-endpoint-proxy** (distributed as a nuget global tool) will be used to generate private nuget packages for the data endpoint, to be used by the `consumer service`. It will generate the following two nuget package.

#### {Publisher}Service.{Consumer}EndPoint
This package will be referenced by the consumer service's `Website` project, in the `Startup.cs` file to kick start the engine.

#### {Publisher}Service.{Consumer}EndPoint.MSharp
This package will be referenced by the consumer service's `#Model` project to enable the necessary code generation.

