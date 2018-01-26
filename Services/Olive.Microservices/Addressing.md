# Olive Microservice Urls
You will typically have 3 environments in a microservice solution:


| Environment  | Microservice Urls | Notes
| ------------- | ------------- | ------------- 
| Local (dev)  | http://serviceA.my-solution.dev.co | The domain should be mapped to 127.0.0.1 in your HOSTS file
| Staging (UAT) | https://serviceA.my-solution.uat.co |
| Production (Live) | https://serviceA.my-solution.com  |

To run the application and test it you will need to run not only *serviceA* but also *serviceB (e.g. Theme)*, *serviceC (e.g. Auth)*, etc. And they all need to work with the same environment.

For example if you want to test the local version of *serviceA* which is hosted on *http://serviceA.my-solution.dev.co* you also have to login to http://auth.my-solution.dev.co and can't use https://auth.my-solution.com because the session cookies won't be compatible. The same applies to Api calls, test data, etc. 

So to keep things simple and clean you need to select an environment to run a service, and rest assured that the correct version of all other related services are being used during your testing.

## Environment definitions
When you create a new Olive Microservice, as part of the template under the *Website* folder there are 3 files that define the configuration variables for your 3 environments.

### *appsettings.json* which is used for local (dev) execution:
```json
 "Microservice": {
        "Root.Domain": "my-solution.dev.co",
        "Http.Protocol": "http"
    }   
```
### *appsettings.Staging.json* which is used for Staging (UAT) execution:
```json
 "Microservice": {
        "Root.Domain": "my-solution.uat.co",
        "Http.Protocol": "https"
    }   
```
### *appsettings.Production.json* which is used for Production (Live) execution:
```json
 "Microservice": {
        "Root.Domain": "my-solution.mydomain.com",
        "Http.Protocol": "https"
    }   
```

> **Microservice URL**: Each microservice in your solution will be defined by a **unique name**, which is a sub-domain under the current environment's root domain. To get the full Url of a microservice by its unique name use:
```csharp
// Get the correct full url to the serviceX microservice: 
string fullUrl = Olive.Microservice.Url("serviceX");

// Or get the url to a specific api path inside that:
string apiUrl = Olive.Microservice.Url("serviceX", "some/relative/path");
```

# Service dependencies during development
When developing a microservice, it often has dependencies on other microservices. As a minimum, it's probably dependent on:
- Theme
- Auth
- People (via Auth)
- Access Hub
- ... *(Depending on what it does it may need other services too.)*

So how can you have those services available so you can get to testing your own service?

### Option 1
One option is to host all of the above on your development machine. But it can be a bit painful. Apart from that, the source code of some of the services you depend on may not even be available to you. Or they may have their own dependencies and setup complications.

### Option 2 (preferred)
The other option is to use a **hosted version of the dependable services** on a development server which is available via internet. This way when developing your own service you can simply use those to go about your testing.

> To achieve this in your local HOSTS file you should map the full url to those services to the IP address of the dev server. This should be done both on the dev server's hosts file as well as your development machine's.

For example if your dev server's ip is 192.168.0.100 you should add:
***auth.my-solution.dev.co     192.168.0.100
theme.my-solution.dev.co     192.168.0.100***

# Domains and Ports configuration
When developing ASP.NET core applications you can run the project by pressing F5 in Visual Studio. When you do that, it will automatically run the following command:
> C:\...\my-solution\my-service\website> **dotnet run**

At this stage, the *dotnet* command line utility will do the following:

1. Compile the web application.
2. Load *Website\Properties\**LaunchSettings.json*** file.
3. Read the *applicationUrl* value.
4. Find the port number from the URL.
   - Note: It ignores the rest of the URL.
   - So whether you use http://localhost:9014 or http://anything.dev.co:9014 doesn't matter to it.
5. Start a [Kestrel web server](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?tabs=aspnetcore2x) web server process and listen on the port for incoming requests.
   - Warning: The port number should not be already in use by IIS or any other running Kestrel web server instances.
   - For example if you use the same port number on multiple running ASP.NET applications this will break.

So you can send HTTP requests to the application on that port from your browser and test the application.

## Domain forwarding and IIS config
During development you need to be able to access your microservices using just the domain and the default port (80).
Unfortunately Kestrel doesn't support host names and only cares about the port number, which also cannot be 80 as that's used by IIS.

> As a workaround you need to set up a special IIS application to act as a reverse proxy. That special IIS application will receive the requests on e.g. http://something.dev.co and pass it on to http://localhost:9014, and then receives the response and pass it on to the browser or HttpClient (in Api calls).

To the caller (browser or api client) it's as if the application is simply responding to http://something.dev.co and there is no port involved.

To set it up in IIS:
1. Create a folder, e.g. C:\inetpub\map\
2. Create an IIS application on that folder.
3. Add bindings for all your microservice domains (e.g. something.my-solution.dev.co) on port 80.
4. Add a web.config file based on the following:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <modules>
      <remove name="FormsAuthentication" />
    </modules>
        <rewrite>
            <rules>
                <rule name="{my-service-name}" stopProcessing="true">
                    <match url="(.*)" />
                    <action type="Rewrite" url="http://localhost:9014/{R:1}" />
                    <conditions>
                        <add input="{HTTP_HOST}" pattern="my-service-name.my-solution.dev.co" />	
                    </conditions>
                </rule>              
            </rules>
        </rewrite>
  </system.webServer>
</configuration>
```
5. For each micro-service in your solution that you need to run locally, add a *<rule>* node under **<rules>** with the correct settings.
