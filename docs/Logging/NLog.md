# Olive Mvc using NLog

NLog is a free logging platform for .NET, NETSTANDARD, Xamarin, Silverlight and Windows Phone with rich log routing and management capabilities. NLog makes it easy to produce and manage high-quality logs for your application regardless of its size or complexity.

NLog can process diagnostic messages emitted from any .NET language, augment them with contextual information (date and time, severity, thread, process, environment), format according to your preferences and send to one or more targets.

[See features, examples and configuration](http://nlog-project.org/#targets)

## How to install

1. Add the nuget package Olive.Mvc.NLog to your application.
2. Open StartUp.cs
3. In Configure() method add the following line:

```csharp
public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ...
    app.UseNLog(env);
    // ...
}
```

4. Add a [NLog.config file](https://github.com/nlog/nlog/wiki/Configuration-file) to your website root and customise it based on your requirements.
