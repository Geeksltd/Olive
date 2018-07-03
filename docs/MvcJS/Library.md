# Loading javascript modules and libraries dynamically
Sometimes you need to create custom javascript modules that are loaded for specific pages as opposed to the whole application.

For example if your script file is under wwwroot/scripts/components/myScript.js you can load it by running the following Javascript code:

```javascript
window.loadModule('/scripts/components/myScript');
```

Or if you want to run a static function of your module named *Run* as soon as it's loaded, you can use:

```javascript
window.loadModule('/scripts/components/myScript', m => m.default.Run());
```
To achieve this in M#, instead of writing the code shown above, in the Page definition file where you want this script to be loaded, simply call the *JavaScript()* method as shown below:
```csharp
public class SomePage : RootPage
{
    public SomePage()
    {
		OnBound("Load module").Code("JavascriptModule.Relative(\"scripts/components/myScript.js\"");
        ...
    }
}
```
M# will generate this code:
```c#
[NonAction, OnBound]
public async Task OnBound(vm.EventSourcesList info)
{
    // Load module
    JavaScript(JavascriptModule.Relative("scripts/components/myScript.js"));
	...    
}
```
*JavascriptModule* has two important static methods, `Absolute` and `Relative`. If you are developing a micro-service and it has a JS file in its scripts folder, then you must use `Absolute` method to load this file from current micro-service, but if you want to load a JS file from *Hub* project, then you must use `Relative` method.

sometime your modules may need some 3rd party libraries or dependencies. for this purpose you should first add all needed libraries in bower.json and then use this code:
```csharp
public class SomePage : RootPage
{
    public SomePage()
    {
		OnBound("JS dependency").Code("var module = JavascriptModule.Absolute(\"scripts/myScript.js\").Add(JavascriptDependency.Absolute(\"lib/fullcalendar/dist/fullcalendar.js\")); JavaScript(module);");
        ...
    }
}
```
M# will generate this code:
```c#
[NonAction, OnBound]
public async Task OnBound(vm.EventSourcesList info)
{
    // JS dependency
    var module = JavascriptModule.Absolute("scripts/myScript.js")
						.Add(JavascriptDependency.Absolute("lib/fullcalendar/dist/fullcalendar.js")); 
	
	JavaScript(module);
	...
}
```
here **Olive** will first load and inject JS dependency named `fullcalendar.js` and then run the module named `myScript.js`.