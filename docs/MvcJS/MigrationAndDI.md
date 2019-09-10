# Migration to version 2 (DI approach)

Normally if you do not use custom client side logic you do not need to do anything and 
upgrading to the version 2 should be as easy as upgrading the package. But if you have
some custom logic or modules you may need to apply some changes.

## Changing the modules
You need to take different actions base on your modules.

### Independent module
An independent module is a module which does not use a service from the Olive Fx or 
another service module in the project. If you have an independent module you would 
not need to do anything about it.
```ts
import NotAServiceModule from "./NotAServiceModule"

export default class CustomModule{

    public static run() {
        //...
    }
}
```


### Dependent module
A dependent module is a module which depends on another service module. As a result, 
it should be changed to a service, itself. To do so, you should follow these steps:
1. Create a variable to keep the service keys. 
    > This step is optional but if you need to load your service in another `.ts` it's 
    better to have it.

Exporting the variable:
```ts
const MyServices = {
    CustomModule: "custom-module"
}

export default MyServices;
```


2. Change the module file:
   * Implement `IService`
   * Change static members to non-static
   * Add a constructor to have have access to the requires services
   
Before:
```ts
import Waiting from "olive/component/waiting";

export default class CustomModule {
	public static process(){
		Waiting.show();
		// ...
		Waiting.hide();		
	}
}
```

After:
```ts
import Waiting from "olive/component/waiting";

export default class CustomModule implements IService {
	constructor(private waiting: Waiting){}
	
	public process(){
		this.waiting.show();
		// ...
		this.waiting.hide();		
	}
}
```

3. Configure your service. To do so, you should override the `configureServices(
services: ServiceContainer)` method in the `appPage.ts`. You should provide a 
callback function which create a new instance of your service and specify all its 
dependencies.

```ts
import Waiting from 'olive/components/waiting';
import CustomModule from './custom/custommodule';
//... rest of imports

export default class AppPage extends OlivePage {
	configureServices(services: ServiceContainer) {
		// You can use 'tryAddSingleton' if you think the 'AppPage' could be 
		// extended and your service may altered.
		services.addSingleton(MyServices.CustomModule, 
			(waiting: Waiting) => new CustomModule(waiting)
		)
		.withDependencies(Services.Waiting);
	}

	//rest of codes
}
```



### Complex Dependent module
A complex dependent module is a module which contains loads of static logic as well as 
non-static logic. Although, it is not of the best design but is something that could 
happen. As mixing up these two parts together could be hard and time-consuming, you 
can split it up into two different modules. To do so, you need to move all the static 
members to a new module. Also, you need to register only the ones which are used as a 
dependency in another service.
   
Before:
```ts
import Waiting from "olive/component/waiting";

export default class CustomModule {
	input: JQuery;
	static isProcessing: bool;

	public static process(){
		Waiting.show();
		// ...
		Waiting.hide();		
	}

	public static CreateInstance(selector: JQuery){
		selector.each((i, e) => new CustomModule($(e)).show()); 
	}

	public constructor(target: JQuery){
		this.input = target;
	}

	public show(){
		//...
	}
}
```

After:
```ts
import Waiting from "olive/component/waiting";

export default class NewCustomModule implements IService {	
	isProcessing: bool;

	constructor(private waiting: Waiting){}
	
	public process(){
		this.waiting.show();
		// ...
		this.waiting.hide();		
	}

	public CreateInstance(selector: JQuery){
		selector.each((i, e) => new CustomModule($(e)).show()); 
	}
}

export class CustomModule {	
	constructor(private input: JQuery){}

	public show(){
		//...
	}
}
```



## Loading a module from Controller or ViewComponent
If you use to load your module from the M# UI module you may:
* Need to do nothing if your module was a independent module and you have not changed it.
* Need to change the way you were loading it as describe below.
	* Change `LoadJavascriptModule` to `LoadJavascriptService`
    * Make sure the arguments parameter results in the currect types.

Before:
```csharp
LoadJavascriptModule("scripts/custom/custommodule.js", 
	"doSomething($\"{info.Item.LoadUrl}, {info.Item.UseIframe.ToString().ToLower()})\");")
	.Criteria(@"info.Item?.ImplementationUrl.HasValue() == true && 
			info.Item?.UseIframe == true");
```

Before:
```csharp
LoadJavascriptService("custom-module", "doSomething", cs("info.Item.LoadUrl, info.Item.UseIframe"))
	.Criteria(@"info.Item?.ImplementationUrl.HasValue() == true && 
			info.Item?.UseIframe == true");
```

For microservice project check [here](../Microservices/LoadTypescriptServiceFromController.md)


## Altering the built in functionalities.
Sometimes you need to change a built in method or module in your project. For instance, 
you may need to change how the redirection errors are handled with 
`AjaxRedirect.onRedirectionFailed`. In this scenario you may had something like the
followin code some where in you codes.

```ts
AjaxRedirect.onRedirectionFailed = (url, response) => {
	// your logic
};
```

You would need to extend the `AjaxRedirect` in your project and register it in your
`appPage.ts` just like the other services.
> There is an existing variable for the built-in services named Services.

Extended service:
```ts
import AjaxRedirect from "olive/mvc/ajaxRedirect";
import Url from "olive/components/url";
import ResponseProcessor from "olive/mvc/responseProcessor";
import Waiting from "olive/components/waiting";

export default class HubAjaxRedirect extends AjaxRedirect {
	constructor(url: Url, responseProcessor: ResponseProcessor, waiting: Waiting) {
		super(url, responseProcessor, waiting);
	}

	protected onRedirected(title: string, url: string) {
		// your logic
	}

	protected onRedirectionFailed(url: string, response: JQueryXHR) {
		// your logic
		super.onRedirectionFailed(url, response);
	}
}
```

How we registering it:
```ts
export default class AppPage extends OlivePage {
	configureServices(services: ServiceContainer) {
		services.addSingleton(Services.AjaxRedirect, 
			(url: Url, responseProcessor: ResponseProcessor, waiting: Waiting) =>
				new HubAjaxRedirect(url, responseProcessor, waiting)
		).withDependencies(
			Services.Url, 
			Services.ResponseProcessor, 
			Services.Waiting);

		super.configureServices(services);
	}

	//rest of codes
}
```
