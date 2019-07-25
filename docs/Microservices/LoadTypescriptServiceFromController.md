# Loading a typescript service module from controller

Read [this article](../MvcJS/MigrationAndDI.md#loading-a-module-from-controller-or-viewcomponent) first.


In order to load a typescript service module from controller you need to make sure:
* You have the `Olive.Mvc` >= `v2.1.133` and `Olive.Mvc.Microservices` >= 
`v2.1.104` in your project.
* Your controllers in are drived from `Olive.Mvc.Microservices.Controller`
* You have configured the services currectly in your project.

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    //...
    services.AddTSConfiguration();
}
```
* You have created the configure services file in your project. To do so, you need 
to create a typescript file name `configureServices.ts` which would be like:

```typescript
//...
import OlivePage from "olive/olivePage";
import { ServiceDescription } from "olive/di/serviceDescription";

export default class ConfigureServices {
    public static configure(services: ServiceContainer) {
        services.addSingleton(MyServices.CustomModule, 
            (waiting: Waiting) => new CustomModule(waiting)
        )
        .withDependencies(Services.Waiting);
    }
}

ConfigureServices.configure((<OlivePage>window.page).services);
```

> Note that if due to any reason you needed to have this file with any other name, you need to pass the valid name 
as the prameter to **AddTSConfiguration**.

* In your modules and service configuration file make sure you have imported with the `.js` extension.

```typescript
// For the framework's files use
import OlivePage from "olive/olivePage";
import { ServiceDescription } from "olive/di/serviceDescription";
// But for the project's file use
import CustomModule from './custom/custommodule.js';
```
