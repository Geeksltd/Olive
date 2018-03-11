# Olive Microservices: Integration

You can create yor APIs using the ASP.NET Web Api architecture which is a very powerful technology.
It's designed to be cross-platform compatible, meaning that your APIs can be invoked by any consumer application written in any technology (.Net, Java, Javascript, Php, ...) via Http.

If you're building an internal business application however, there is a good chance that **most, if not all, of your Apis will be used only by other .NET microservices within the same solution**.

## The problem with normal Api calling

The .NET framework provides you with the *HttpClient* class to invoke Http based web-apis.
In addition, Olive provides the **ApiClient** class which [provides a lot of benefits on top of that](../Api/ApiClient.md)
and solves many routine problems for you.

Yet still, for consuming remote services, there are some problems remaining, such as:

- Create DTO classes for request and response types
- Making sure the Api Urls are up to date
- Making sure the correct version of the service is being invoked (Dev v.s. Staging v.s. Production)


# Solution: Olive Api Proxy Generator

To free you from the problems mentioned above, there is a utility named **Olive.ApiProxy** that can automatically creates a proxy Dll for your Apis. You will then install that dll in the consumer services and use it to access the API with maximume ease.

It will automatically take care of all of the problems mentioned above and gives you a strongly typed and simplified approach to **invoke remote Apis as if they are local functions in the same consumer service**.

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

# Generating an Api Proxy (Olive.ApiProxyGenerator.dll)

1. Ensure you have a NuGet reference to [Olive.ApiProxyGenerator](https://www.nuget.org/packages/Olive.ApiProxyGenerator/)
2. Compile the website project (which includes the Api code)
3. Right click on the Api controller class in Visual Studio solution explorer.
4. Select "Generate Proxy Dll..." which opens a pop-up
5. Provide the requested settings to define the target destination for publishing the generated Proxy.

> The Api.Proxy.dll tool will generate two class library projects, one called **Proxy** and one called **MSharp**. It will then compile them and generate a nuget package for each, and publish to the requested destination. The generated NuGet packages can then be referenced in the consuming microservices.

## Using the generated proxy

In the consumer application (service) reference the generated NuGet package.
You can then create a proxy object and then call the remote Api function. For example:

```csharp
var result = await new MyPublisherService.MyApi().MyFunction(...);
```

## Api input and output types (schema)

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

## Security

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

### Impersonating the (human) user

Sometimes you need to call an Api in a microservice, but you need the Api to be invoked with the identity of the current HTTP user, rather than the client service.

To achieve that, when creating an Api Proxy instance, invoke AsHttpUser() before calling the Api function. For example:

```csharp
var result = await new MyPublisherService.MyApi().AsHttpUser().MyFunction(...);
```

When you do this, the cookies from the current http request will be forwarded in the Http call to the Api publishing service.
