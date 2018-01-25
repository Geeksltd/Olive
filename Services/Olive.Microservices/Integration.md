# Olive Microservices: Integration
You can create yor APIs using the ASP.NET Web Api architecture which is a very powerful technology.
It's designed to be cross-platform compatible, meaning that your APIs can be invoked by any consumer application written in any technology (.Net, Java, Javascript, Php, ...) via Http.

If you're building an internal business application however, there is a good chance that **most, if not all, of your Apis will be used only by other .NET microservices within the same solution**.

## The problem with normal Api calling
Of course the .NET framework provides you with standard classes to invoke Http based web-api services.
You can see [examples here](https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client) for how to use HttpClient and related classes to achieve this.

However that way you have to write a lot of code which can be time consuming and error prone. You also have to deal with many issues yourself:
- Caching 
- Exception handling and logging
- Making sure the Api Urls are up to date
- Making sure the correct version of the service is being invoked (Dev v.s. Staging v.s. Production)
- Create DTO classes for request and response types

## Solution: Olive Api Proxy Generator
To free you from the problems mentioned above, there is a utility named **OliveApiProxyGenerator** that can automatically create a proxy Dll for your Apis. You will then install that dll in the consumer services and use it to access the API with maximume ease.

It will automatically take care of all of the problems mentioned above and gives you a strongly typed and simplified approach to **invoke remote Apis as if they are local functions in the same consumer service**.

## Creating an Api
To benefit from the architecture and tools explained here you should use the pattern explained here for implementing your Apis.

```csharp
namespace {PublisherName}Service
{
    [Route("api")]
    public class {SomeName}Api : BaseController
    {
        [HttpGet, Route("...")]
        [Returns(typeof(MyReturnType)]
        // Todo: add the required [Authorize(Role=...)] settings
        public async Task<IActionResult> MyFunctionName(string someParameter1, stringsomeParameter2)
        {
            // TODO: Add any custom validation
            if ({Some invalid condition}) return BadRequest();

            MyReturnType result = ....;

            return Json(result);
        }
    }
    
    public class MyReturnType
    {
         public string Property1 {get; set;}
         public int Property2 {get; set;}
         ...
    }
}
```
### Main points
- Place this file in ***Website\Api*** folder
- The **namespace** should be the name of the micro service that publishes the Api followed by *Service*. 
- The

### Tips
- Add the appropriate [Authorize] 

**Step 1**
In the publisher service under *Website\Api* folder add a new class called ***XxxxApi.cs***.
