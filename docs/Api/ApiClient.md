# ApiClient: Calling Web Apis

ASP.NET Web Apis are based on the standard HTTP protocol, which means any http client from any technology can use them.

## .NET based client app
To consume a Web Api in a .NET client app you can use the *HttpClient* class provided by the .NET framework.

HttpClient provides a basic set of methods for sending HTTP requests to any URL, and receiving the responses.
You can see [examples here](https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client) to learn how to use HttpClient.

When using HttpClient directly, you should manually take care of:
* Serialization of CLR request parameters
* Deserialization of response objects to strongly typed CLR types
* Http error code handling
* Application error message Json response handling
* Security and authentication cookies
* Caching GET Api responses
* Cache invalidation
* Handling temporary failures 
  * Queuing POST, PUT, DELETE and PATCH messages
  * Retries for GET requests
  * Circuit breaking (to avoid server overloading)

## Olive.ApiClient
Olive provides a helper utility class named [ApiClient](https://github.com/Geeksltd/Olive/tree/master/Integration/Olive.ApiClient) which handles all of the above issues for you.
It's built on top of the standard HttpClient, and is ideal for invoking Web Apis.

The following simple line is enough to take care of all aspects of downloading a customer object from a remote Web Api.

```csharp
var customer = await new ApiClient($"{baseUrl}/customers/{id}").Get<Customer>();
```

## Result object deserialization
The above example assumes that the Web Api will return som json data that is compatible with the schema of the *Customer* class in the client application.
It will automatically deserialize the json result to that type.

For **collections**, we recommend using an Array type instead of IEnumerable or List. This can make it very clear to the consumer code that the result is already evaluated and is read-only.
For example:
```csharp
var customers = await new ApiClient($"{baseUrl}/customers").Get<Customer[]>();
```

---

## Sending query string arguments
To send query string parameters to the target Web Api, simply add them as properties of an anonymous object, and pass it as the Get() method attribute.
```csharp
var customers = await new ApiClient($"{baseUrl}/customers")
                         .Get<Customer[]>(new { category = myCategoryId });
```
---

## Cache vs Fresh
Remote services can be faulty, slow or unresponsive at times due to network, server or application problems.

By default, Olive will cache the response of every Get request. The raw textual result (often Json data) will be saved upon every successful execution of that Api.
The name of the cache file will come from the URL (HASHED) so that subsequent successful calls to the same url will simply overwrite the same file.

When calling GET based Apis, you have a choice (trade-off) to make in relation to reliability vs freshness of the response data.
To specify your choice, you simply call the Cache(...) method on ApiClient:

```csharp
var customers = await new ApiClient($"{baseUrl}/customers")
                         .Cache(CachePolicy.FreshOrCacheOrFail) // <- Default if not set
                         .Get<Customer[]>();
```

> The Cache() method also accepts an optional *TimeSpan* variable named **cacheExpiry**. When set, if the cached file (from a previous Api response) is older than that, then the cache will be ignored.

You have 8 options:

| Option  | When your priorities are | And you're happy to accept
| ------------- | ------------- | ----------
| FreshOrCacheOrFail  | **Up-to-date** then **Minimum crashing** | *Lower speed*
| FreshOrCacheOrNull  | **Up-to-date** then **No crashing** | *Lower speed*
| CacheOrFreshOrFail  | **Speed** then **Minimum crashing** | *Relatively out-of-date result*
| CacheOrFreshOrNull  | **Speed** then **No crashing** | *Relatively out-of-date result*
| FreshOrFail  | **Must be up-to-date** | *Lower speed and more crashing*
| FreshOrNull  | **Must be up-to-date** | *Lower speed, no crashing and maybe null result*
| CacheOrFail  | **Speed** | *Relatively out-of-date result and more crashing*
| CacheOrNull  | **Speed** | *Relatively out-of-date result, no crashing and maybe null result*

#### CachePolicy.FreshOrCacheOrFail (default)

This is the safest option. It is likely to give you up to date result most of the time.
But it's not the fastest option as it always waits for a remote call before returning.
This is how it works:
 
* Make a fresh HTTP request and wait for the response.
   * Successful? update the cache file, and return the response.
   * Failed? Is there a cached file from before? 
     * Yes? Return from cache.
     * No? Throw the exception. 

#### CachePolicy.FreshOrCacheOrNull

This is exactly like **CachePolicy.FreshOrCacheOrFail** but if an error happens it doesn't throw an exception and just return *null*.
This is how it works:
 
* Make a fresh HTTP request and wait for the response.
   * Successful? update the cache file, and return the response.
   * Failed? Is there a cached file from before? 
     * Yes? Return from cache.
     * No? Return null. 

#### CachePolicy.CacheOrFreshOrFail
This is the fastest option, and is the least likely to crash.
Use this if you can tolerate response data that may not be up-to-date.
This is how it works:
 
* Is there a cached file from before?
   * Yes? return from cache.
   * No? Make a fresh HTTP request and wait for the response.
      * Successful? Update the cache file, and return the response.
      * Failed? Throw the exception. 

#### CachePolicy.CacheOrFreshOrNull
This is exactly like **CachePolicy.CacheOrFreshOrFail** but if an error happens it doesn't throw an exception and just return *null*.
This is how it works:
 
* Is there a cached file from before?
   * Yes? return from cache.
   * No? Make a fresh HTTP request and wait for the response.
      * Successful? Update the cache file, and return the response.
      * Failed? Return null. 

#### CachePolicy.FreshOrFail
Choose this if you only want up-to-date data and want to ignore the cache, even if it means crashing.
This is how it works:
 
* Make a fresh HTTP request and wait for the response.
   * Successful? update the cache file, and return the response.
   * Failed? Throw the exception.

#### CachePolicy.FreshOrNull
This is exactly like **CachePolicy.FreshOrFail** but if an error happens it doesn't throw an exception and just return *null*.
This is how it works:
 
* Make a fresh HTTP request and wait for the response.
   * Successful? update the cache file, and return the response.
   * Failed? Return null.

#### CachePolicy.CacheOrFail
This is the fastest option. Use this if you plan to get just cached data and never make any new request.
This is how it works:
 
* Is there a cached file from before?
   * Yes? return from cache.
   * No? Throw the exception.


#### CachePolicy.CacheOrNull
This is exactly like **CachePolicy.CacheOrFail** but if an error happens it doesn't throw an exception and just return *null*.
This is how it works:
 
* Is there a cached file from before?
   * Yes? return from cache.
   * No? Return null.

*Note:* The cache file will still be updated, in case you want to invoke the same Api with a different cache policy in the future.

---

## Show message
If a call to a Web API results in an error or you want to inform the user about using cache result instead of fresh result you can use `.OnFallBack()` method. By default a toast message will be shown to the user that contain related information.
You can set a fallback policy on your ApiClient by using:

```csharp
var customers = await new ApiClient($"{baseUrl}/customers")
                         .OnFallBack(ApiFallBackEventPolicy.Raise) // <- Default if not set
                         .Get<Customer[]>();
```

#### ApiFallBackEventPolicy.Raise
This will bring a toast message to the user.

#### ApiFallBackEventPolicy.Silent
This will hide toast message and the user will not see any messages.

Your choice here is relevant in relation to the cache policy and the actuality of the situation:

| Cache Policy  | OnFallBack | Api failed, cache available | Api failed, cache unavailable
| ------------- | ------------- | ---------- | -------
| FreshOrCacheOrFail  | Raise | (from cache), toast message | **Exception**
| FreshOrCacheOrNull  | Raise | (from cache), toast message | *NULL*, toast message
| CacheOrFreshOrNull  | Raise | (from cache) | *NULL*, toast message
| FreshOrNull  | Raise | --- | *NULL*, toast message
| CacheOrNull  | Raise | (from cache) | *NULL*, toast message

#### Notification
Notification here means *showing a toast message to the user*. This option is only applicable when:
* The Api client app is an ASP.NET app itself.
* The call to ApiClient is running within an active Http request. 

> The notification will be registered in the ASP.NET pipeline, and displayed on web page when the response is sent

This is a handy option for when: 
* You don't want to show an error screen if the remote Api has crashed
* But you want to inform the user that the result they see on the page may be out of date.

## Resiliency
You can set retry and circuit breaker settings for a more resilient and fault tolerant integration.
```csharp
var customers = await new ApiClient($"{baseUrl}/customers")
                         .Retries(3)
                         .CircuitBreaker(exceptionsBeforeBreaking: 5, breakDurationSeconds: 10)
                         .Get<Customer[]>();
```

#### What is Circuit Breaker?
It prevents sending too many requests to an already failed remote service.
If http exceptions are raised consecutively for the specified number of times, it will
***break the circuit*** for the specified duration.

During the break period, any attempt to execute a new request will **immediately throw a BrokenCircuitException**.
Once the duration is over, if the first action throws http exception again,
the circuit will break again for the same duration. Otherwise the circuit will reset.
