# ApiClient: Calling Web Apis

ASP.NET Web Apis are based on the standard HTTP protocol, which means any http client from any technology can use them.

### .NET based client app
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

### Olive.ApiClient
Olive provides a helper utility class named ***ApiClient*** which handles all of the above issues for you.
It's built on top of the standard HttpClient, and is ideal for invoking Web Apis.

The following simple line is enough to take care of all aspects of downloading a customer object from a remote Web Api.

```csharp
var customer = await new ApiClient($"{baseUrl}/customers/{id}").Get<Customer>();
```

### Result object deserialization
The above example assumes that the Web Api will return som json data that is compatible with the schema of the *Customer* class in the client application.
It will automatically deserialize the json result to that type.

For **collections**, we recommend using an Array type instead of IEnumerable or List. This can make it very clear to the consumer code that the result is already evaluated and is read-only.
For example:
```csharp
var customers = await new ApiClient($"{baseUrl}/customers").Get<Customer[]>();
```

### Sending Query String Arguments
To send query string parameters to the target Web Api, simply add them as properties of an anonymous object, and pass it as the Get() method attribute.
```csharp
var customer = await new ApiClient($"{baseUrl}/customers/{id}").Get<Customer>(new { queryStringParameter1 = value1 } );
```