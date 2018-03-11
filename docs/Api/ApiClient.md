# ApiClient: Calling Web Apis

ASP.NET Web Apis are based on the standard HTTP protocol, which means any http client from any technology can use them.

## .NET based client app
To consume a Web Api in a .NET clietn app you can use the *HttpClient* class provided by the .NET framework.

HttpClient provides a basic set of methods for sending HTTP requests to any URL, and receiving the responses.

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
Olive provides a helper utility class named ***ApiClient*** which handles all of the above issues for you.
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
Remove services can be faulty, slow or unresponsive at times due to network, server or application problems.

By default, Olive will cache the response of every Get request. The raw textual result (often Json data) will be saved upon every successful execution of that Api.
The name of the cache file will come from the URL (HASHED) so that subsequent successful calls to the same url will simply overwrite the same file.

When calling GET based Apis, you have a choice (trade-off) to make in relation to reliabilty vs freshness of the response data.
To specify your choice, you simply call the Cache(...) method on ApiClient:

```csharp
var customers = await new ApiClient($"{baseUrl}/customers")
                         .Cache(CachePolicy.FreshOrCacheOrFail) // <- Default if not set
                         .Get<Customer[]>();
```

You have 3 options:

| Option  | When your priorities are | And you're happy to accept
| ------------- | ------------- | ----------
| FreshOrCacheOrFail  | **Up-to-date** then **Minimum crashing** | *Lower speed**
| CacheOrFreshOrFail  | **Speed** then **Minimum crashing** | *Relatively out-of-date result**
| FreshOrFail  | **Must be up-to-date** | *Lower speed and more crashing**

#### CachePolicy.FreshOrCacheOrFail (default)

This is the safest option. It is likely to give you up to date result mos of the time.
But it's not the fastest option as it always waits for a remote call before returning.
This is how it works:
 
* Make a fresh HTTP request and wait for the response.
   * Successful? update the cache file, and return the response.
   * Failed? Is there a cached file from before? 
     * Yes? Return from cache.
     * No? Throw the exception. 

#### CachePolicy.CacheOrFreshOrFail
This is the fastest option, and is the least likely to crash.
Use this if you can tolerate response data that may not be up-to-date.
This is how it works:
 
* Is there a cached file from before?
   * Yes? return from cache.
   * No? Make a fresh HTTP request and wait for the response.
      * Successful? Update the cache file, and return the response.
      * Failed? Throw the exception. 

#### CachePolicy.FreshOrFail
Choose this if you only want up-to-date data and want to ignore the cache, even if it means crashing.
This is how it works:
 
* Make a fresh HTTP request and wait for the response.
   * Successful? update the cache file, and return the response.
   * Failed? Throw the exception.

Note: The cache file will still be updated, in case you want to invoke the same Api with a different cache policy in the future.

---

## Error handling
If a call to a Web Api results in an error, by default you will get an exception thrown with the correct error message.
You can set an error policy on your ApiClient by using:
```csharp
var customers = await new ApiClient($"{baseUrl}/customers")
                         .OnError(OnApiCallError.Throw) // <- Default if not set
                         .Get<Customer[]>();
```

Your choice here is relevant in relation to the cache policy and the actuality of the situation:

| Cache Policy  | OnError | Api failed, cache available | Api failed, cache unavailable
| ------------- | ------------- | ---------- | -------
| FreshOrCacheOrFail  | Throw | (from cache) | **Exception**
| FreshOrCacheOrFail  | Ignore | (from cache) | *NULL*
| FreshOrCacheOrFail  | IgnoreAndNotify | (from cache), toast message | *NULL*, toast message
| CacheOrFreshOrFail  | Throw | (from cache) | **Exception**
| CacheOrFreshOrFail  | Ignore | (from cache) | *NULL*
| CacheOrFreshOrFail  | IgnoreAndNotify | (from cache) | *NULL*, toast message
| FreshOrFail  | Throw | **Exception** | **Exception**
| FreshOrFail  | Ignore | *NULL* | *NULL*
| FreshOrFail  | IgnoreAndNotify | *NULL*, toast message | *NULL*, toast message

#### IgnoreAndNotify
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