# Api Proxy Generator

The **ApiClient** class will solve many of the routine integration problems for you.
But for consuming remote services, there are still some problems remaining, such as:

In addition there is a utility named **generate-api-proxy** to help simplify invoking Web Apis. It is available as a *DotNet Global Tool* distributed via Nuget.

It generates a proxy assembly and NuGet package for your Web Api, to be installed in the consumer app.
The generated proxy assembly will:

- Provide a strongly typed proxy class to invoke remote services as if they are local functions.
- Provide strongly typed DTO classes for Web Api parameters and response types
- Automatically take care of composing the correct Api Url
- Make sure the correct version of the service is being invoked (Dev v.s. Staging v.s. Production)


## Creating an Api

To benefit from the architecture and tools explained here you should use the pattern explained here for implementing your Apis.
The following file should be created in ***Website\Api*** folder:

```csharp
namespace MyPublisherService
{
    [Route("api")]
    public class MyApi : BaseController
    {
        [HttpGet, Route("...")]
        [Returns(typeof(MyReturnType)]
        // Todo: add the required [Authorize(Role=...)] settings
        public async Task<IActionResult> MyFunction(string someParameter1, stringsomeParameter2)
        {
            // TODO: Add any custom validation
            if ({Some invalid condition}) return BadRequest();

            MyReturnType result = ....;
            return Json(result);
        }
        public class MyReturnType
        {
            public string Property1 {get; set;}
            public int Property2 {get; set;}
            ...
        }
    }
}
```

### Namespace: MyPublisherService

The namespace of your Api class should be the name of the microservice that publishes the Api followed by *"Service"*. For example PeopleService, AuthService, OrderService...

The same namespace will be used in generating the proxy dll for use in the Api consumers. This way they know exactly which microservice they are calling the Api from.


### Controller class name: MyApi

The controller class name should be the **logical name** of this particular Api followed by *"Api"* and named after the purpose that it serves for *one particular consumer*.

> Aim to create **one Api controller class per consumer micro-service** and even name it after that consumer.

**Warning:** Of course the are cases where multiple consumer microservices seem to need the same thing, and you might be tempted to generalise the Api to be used by all of them. However **beware** that:

- Sharing one Api with multiple consumers makes it hard to change and *adapt it overtime to suit the requirements of each particular consumer*, since a change that is desirable for consumer A could break consumer B.
- Each consumer may need different fields of data. To satisfy everyone's need you may have to over-expose data in a shared Api to satisfy everyone. But that can cause security issues as well as inefficiency.

> Click [Here](https://geeksltd.github.io/Olive/#/Api/WebApi) for more information and practices.


### Return type

As you can see, the return types are *nested* classes. So what if we want to use a *real* class right here? The answer is easy, you can take advantage of polymorphism.
So we can make a new nested class then inherit from the target class. Just like this:

```csharp
namespace MyPublisherService
{
    [Route("api")]
    public class MyApi : BaseController
    {
        [HttpGet, Route("...")]
        [Returns(typeof(MyReturnType)]
        // Todo: add the required [Authorize(Role=...)] settings
        public async Task<IActionResult> MyFunction(string someParameter1, stringsomeParameter2)
        {
            // TODO: Add any custom validation
            if ({Some invalid condition}) return BadRequest();

            MyReturnType result = new SomeClassOutHere("Some","Parameters");
            // Or mabe a logic that returns a SomeClassOutHere typed object.
            
            return Json(result);
        }
        public class MyReturnType : SomeClassOutHere
        {
            
        }
    }
}
```

## Generating an Api Proxy using `generate-api-proxy`

1. Ensure you have [generate-api-proxy tool](https://www.nuget.org/packages/generate-api-proxy/) installed by calling `dotnet tool install -g generate-api-proxy`
2. Compile the website project (which includes the Api code)
3. Right click on the Api controller class in Visual Studio solution explorer.
4. Select "Generate Proxy Dll..." which opens a pop-up
5. Provide the requested settings to define the target destination for publishing the generated Proxy.

> The generate-api-proxy tool will generate two class library projects in a temp location, one called **Proxy** and one called **MSharp**. It will then compile them and generate a nuget package for each, and publish to the requested destination. The generated NuGet packages can then be referenced in the consuming microservices.

### Using the generated proxy

In the consumer application (service) reference the generated NuGet package.
You can then create a proxy object and then call the remote Api function. For example:

```csharp
var result = await new MyPublisherService.MyApi().MyFunction(...);
```

### Api input and output types (schema)

Your Api functions' arguments and return types may be void, simple .net types (string, int, DateTime, ...) or they may be classes with multiple fields. For example if you have an Api function called GetUserDetails() it will probably need to return a class with several fields.

```csharp
class User
{
    public Guid ID {get; set;}
    public string FirstName {get; set;}
    public string LastName {get; set;}
    ...
}
```

These class definitions (aka schema) should be recognised in the consumer applications also so they can communicate correctly with your Api. To simplify your consumer apps' code **Olive.ApiProxy.dll** will generate a simple DTO class with exactly the same name and properties as the types used in your Api function (either as argument or return type).

Of course it will also generate a proxy class which acts as an agent in the consumer app to connect to your Api. It will have the *same class name and namespace* as the Api controller.
> **Tip: The generated proxy dll:** Feel free to inspect the generated proxy class's code to learn what is under the hood by looking inside the publisher service's Website\obj\api-proxy folder.

#### ToString property
The generated M# entity in the generated nuget package is, well, a normal M# entity. And like all other, it will have a default `ToString()` implementation which will be guessed based on the property names. To specify that yourself, you can use the [ToString] attribute on top of the property that you want at the source. For example:

```csharp
class User
{
    ...    
    [ToString]
    public string LastName {get; set;}
}
```

### Security

When creating a proxy object, by default it will assume a **service identity**. It will read the *AccessKey* value from *appSettings.json* (*which is issued to it by the Api publisher service*) and send it using HTTP HEADER with the key of **"Microservice.AccessKey"**.

```json
...
"Microservice": {
        "Me": {
            "Name": "SomeConsumerService"
        },

        "SomePublisherService": {
            "Url": "http://localhost:9015",
            "AccessKey": "some-token-xyz"
        }
    },
    /// other codes
```

When a Http request is received by the Api host, it will read the header value under the key of **"Microservice.AccessKey"**. That token is then matched against its registered Api Clients in its own *appSettings.json* file:

```json
 ...
 "Authentication": {
        "Api.Clients": {
            "some-token-xyz": [ "Role1", "Role2" ]
        }
    },
 ...
```

It will then create a Http Context user object with the fixed name of ***"Microservice.ApiUser"*** and the roles specified above.
So you can use the standard ASP.NET role based security for your Api definition. For example you can use the usual *[Authorise(Roles=...)]* code in your Web Api class or Action method definition.

#### Impersonating the (human) user

Sometimes you need to call an Api in a microservice, but you need the Api to be invoked with the identity of the current HTTP user, rather than the client service.

To achieve that, when creating an Api Proxy instance, invoke AsHttpUser() before calling the Api function. For example:

```csharp
var result = await new MyPublisherService.MyApi().AsHttpUser().MyFunction(...);
```

When you do this, the cookies from the current http request will be forwarded in the Http call to the Api publishing service.
