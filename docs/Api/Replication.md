# Data Replication

Typically, in microservices architecture, you will have services that need data from other services.
Let's say we have a `publisher service` that owns some data which is needed by a `consumer service`.
For the publisher service to make its data available to the consumer service, there are two primary options:

## Api Approach
In this way, the publisher service will make its data available via an API for the consumer service to invoke.

#### Performance and resiliency
The consumer processes may frequently need access to the publisher's data. If they make an API call every time they need it, it can be very slow. 
But more importantly, what if the publisher service is unavailable? Well, the consumer service will also fail!
This defeats the purpose of using microservices architecture in the first place.

To solve the performance problem, the consumer service can cache the data for a desired period of time.
A support for this is built-in to the Olive `ApiClient` framework.

To solve the resiliency problem, the `ApiClient` framework in Olive, allows reusing the earlier snapshots of the cached data, even when the cache lifetime is passed.

#### Data recency
While the caching approach can somewhat solve the performance and resiliency problem, but it introduces another problem, which is data recency.
In other words, a change in the publisher data will not be seen immediately by the consumer and so its cache will not be automatically updated.

You can, of course, make a two way system where every change in the publisher data would notify the consumer to refresh its cache. 
But that will be: 
- Complex to implement
- Unreliable (what if the consumer service isn't available at that moment?)
- Inefficient (a single record change can invalidate a whole set of cached data unnecessarily)

## Replication Approach
