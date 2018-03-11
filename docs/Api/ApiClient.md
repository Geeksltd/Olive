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

## Sending query string arguments
To send query string parameters to the target Web Api, simply add them as properties of an anonymous object, and pass it as the Get() method attribute.
```csharp
var customers = await new ApiClient($"{baseUrl}/customers").Get<Customer[]>(new { category = myCategoryId });
```

## Cache vs Fresh
Remove services can be faulty, slow or unresponsive at times due to network, server or application problems.

By default, Olive will cache the response of every Get request. The raw textual result (often Json data) will be saved upon every successful execution of that Api.
The name of the cache file will come from the URL (HASHED) so that subsequent successful calls to the same url will simply overwrite the same file.

When calling GET based Apis, you have a choice (trade-off) to make in relation to reliabilty vs freshness of the response data.
To specify your choice, you simply call the Cache(...) method on ApiClient:

```csharp
var customers = await new ApiClient($"{baseUrl}/customers")
                         .Cache(CachePolicy.FreshOrCacheOrFail)
                         .Get<Customer[]>();
```

You have 3 options.
### CachePolicy.FreshOrCacheOrFail (default)
 Choose this if your priorities are: 
 1. Up-to-date result
 2. Minimum crashing
 3. Can compromise on speed and resource efficieny. 

This is the safest option. It is likely to give you up to date result mos of the time.
But it's not the fastest option as it always waits for a remote call before returning.
This is how it works:
 
* Make a fresh HTTP request and wait for the response.
   * Successful? update the cache file, and return the response.
   * Failed? Is there a cached file from before? 
     * Yes? Return from cache.
     * No? Throw the exception. 

### CachePolicy.CacheOrFreshOrFail
 Choose this if your priorities are: 
 1. Fastest response
 2. Minimum crashing, 
 3. Not the most up-to-date response. 

This is how it works:
 
* Is there a cached file from before?
   * Yes? return from cache.
   * No? Make a fresh HTTP request and wait for the response.
      * Successful? Update the cache file, and return the response.
      * Failed? Throw the exception. 

### CachePolicy.FreshOrFail
 Choose this if your priorities are: 
 1. Must be up-to-date
 2. Happy for it to crash otherwise
 3. Speed is also irrelevant

This is how it works:
 
* Make a fresh HTTP request and wait for the response.
   * Successful? update the cache file, and return the response.
   * Failed? Throw the exception.

The cache file will still be updated (in case you want to invoke the same Api with a different cache policy in the future).


## Error handling
If a call to a Web Api results in an error, by default you will get an exception thrown with the correct error message.
