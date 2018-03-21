# Dependency Injection and provider model in Olive

Olive template takes the advantage of *provider model* and *Dependency Injection* (DI) to give the developers a high range of flexibility in the development cycle. If you don't know much about DI and Strategy pattern, first look at [HERE](https://www.codeproject.com/Tips/657668/Dependency-Injection-DI) and [HERE](https://stackify.com/dependency-injection-c-sharp/) (Make sure you are totally cool with full OOP concepts first).

## Provider model

Olive follows *Provider model*. This simply means that developers can use different providers for a certain service. In another word any developer can choose different behaviors for a service.

![image](https://user-images.githubusercontent.com/22152065/37720232-1943a776-2d3c-11e8-9fd5-9112027d97ff.png)

In this example we registered *File provider* anf *Console provider* to a logging service. So if in any application we try to log something, It will be logged by both *File* and *Console* provider.

In real world .NET programs, developers use *Dependency Injection*(DI) for this purpose.There would be an *Interface* representing the service and also implementation of that interface representing the providers.

## How Olive handles DI

Olive deals with DI really simple! Some Olive library and plugins offer some service and provider registration so Olive provides you ways to register and get services.

### How to register and get a service

Navigate to **Startup.cs** file under *App_start* folder of an Olive MVC project. You can simply register your service by acting like this in `ConfigureServices` method:

```csharp
services.AddSingleton<Olive.Audit.ILogger>(new DatabaseLogger());
```

If you want to get a service, use this fluent API of *Olive* get the services:

```csharp
var MyService = Context.Current.ServiceProvider.GetServices<ILogger>();
```

Here we registered the type `DatabaseLogger()` (Which implements `Olive.Audit.ILogger`)  for `Olive.Audit.ILogger` to the DI container. `Context.Current.ServiceProvider.GetServices` returns us a `IEnumerable` of registered services for that interface.

#### Get an specific service

If you want to work with a *specific* type of service or switch between different implementations, you should check the *type* of that implementation with the type you want.

```csharp
var SpecificService = Context.Current.ServiceProvider.GetServices<ILogger>().Where(x => x.GetType() == typeof(DatabaseLogger)).AwaitAll(x => x.Log(auditEvent));
```

### Olive.MVC

If you look at **Startup.cs** carefully you'll notice that It inherits from **Olive.Mvc.Startup** where Olive registers and configures some services as well. You can check out [here](https://github.com/Geeksltd/Olive/blob/master/Mvc/Olive.Mvc/Utilities/Startup.cs) to have an in-depth look into that.