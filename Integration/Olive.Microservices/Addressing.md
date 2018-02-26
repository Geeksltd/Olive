# Olive Microservice Urls
You will typically have 3 environments in a microservice solution:


| Environment  | Microservice Urls | Notes
| ------------- | ------------- | ------------- 
| Local (dev)  | http://localhost:123456 | This address is set under ****. You need a unique port for each microservices in the solution.
| Staging (UAT) | https://serviceA.my-solution.uat.co | Any domain you want to use for staging. Each microservice will usually be a sub-domain. You might even have multiple staging versions, each for a different feature implementation.
| Production (Live) | https://serviceA.my-solution.com  | Similar to Staging, but with the live domain. 

## Sister service dependency and addressing
Let's say you want to run the application and test your *serviceA* which is in development. It might have dependencies on other services such as *serviceB*, *serviceC*, etc. For each of those services, you may also have different versions on different environments.

For example if you want to test the local version of *serviceA* which is hosted on *http://localhost:12345* you need to specify which version of *serviceB* you want to use, depending on what you are trying to test. For example you may want to use its local version on *http://localhost:890* to be safe. But sometimes you may need to test against the live version of serviceB at *https://serviceB.my-solution.com*, which is often only advised for READING data as opposed to sending commands to avoid ruining the live data.

You can use *appSettings.json* to define the sister services you depend on, and specify which URL you want to use at each point. You achieve that under the **Microservice** node in *appSettings.json**:

```json
"Microservice": {
        "Me": {
            "Name": "ServiceA"
        },

        "ServiceB": {
            "Url": "http://localhost:890",
            "AccessKey": "..."
        },
        
        "ServiceC": {
            "Url": "http://localhost:720",
            "AccessKey": "..."
        },
    },
    ...
```


> **Url**: This sets the desired url of a dependee microservice. When you select the localhost address for *ServiceB* you intend to use the version of that service that is running on your local computer. During development you may change this URL several times. For example you might want to test the integration between ServiceA and ServiceB against the local test data, Staging data or even the live data.

> **AccessKey**: The dependee service version that you select should explicitly allow you to access it. For example the live version of ServiceB might have sensitive data that you're not supposed to have access to. So merely setting the Url is not enough to grant your service access to any dependee sister service. That service should generate a unique key for you, and specify the roles associated with that key (this can be done in the *appSettings.json* file of that service under **Authentication:Api.Clients**).


# Domains and Ports configuration
When developing ASP.NET core applications you can run the project by pressing F5 in Visual Studio. When you do that, it will automatically run the following command:
> C:\...\my-solution\my-service\website> **dotnet run**

At this stage, the *dotnet* command line utility will do the following:

1. Compile the web application.
2. Load *Website\Properties\**LaunchSettings.json*** file.
3. Read the *applicationUrl* value.
4. Find the port number from the URL.   
5. Start a [Kestrel web server](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?tabs=aspnetcore2x) web server process and listen on the port for incoming requests.
   - Warning: The port number should not be already in use by IIS or any other running Kestrel web server instances.
   - For example if you use the same port number on multiple running ASP.NET Core apps this will break.

Then you can open a browser and send HTTP requests to the application on that port from your browser and test the application.
