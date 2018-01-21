# Olive Microservice Urls
You will typically have 3 environments in a microservice solution:


| Environment  | Microservice Urls | Notes
| ------------- | ------------- | ------------- 
| Local (dev)  | http://serviceA.ms.me | The domain should be mapped to 127.0.0.1 in your HOSTS file
| Staging (UAT) | https://serviceA.ms.uat.co |
| Production (Live) | https://serviceA.company.com  |

To run the application and test it you will need to run not only *serviceA* but also *serviceB (e.g. Theme)*, *serviceC (e.g. Auth)*, etc. And they all need to work with the same environment.

For example if you want to test the local version of *serviceA* which is hosted on *http://serviceA.ms.me* you also have to login to http://auth.ms.me and can't use https://auth.company.com because the session cookies won't be compatible. The same applies to Api calls, test data, etc. 

So to keep things simple and clean you need to select an environment to run a service, and rest assured that the correct version of all other related services are being used during your testing.


## Environment definitions
When you create a new Olive Microservice, as part of the template under the *Website* folder there are 3 files that define the configuration variables for your 3 environments.

### *appsettings.json* which is used for local (dev) execution:
```json
 "Microservice": {
        "Root.Domain": "ms.me",
        "Http.Protocol": "http"
    }   
```
### *appsettings.Staging.json* which is used for Staging (UAT) execution:
```json
 "Microservice": {
        "Root.Domain": "ms.uat.co",
        "Http.Protocol": "https"
    }   
```
### *appsettings.Production.json* which is used for Production (Live) execution:
```json
 "Microservice": {
        "Root.Domain": "ms.mydomain.com",
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
