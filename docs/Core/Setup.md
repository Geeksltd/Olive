# Using Olive

Olive is used by default in M# project templates. You can also add Olive nuget packages to any other project to use the extension methods.

Some of the Olive features however, will need the core Olive context and services to be set up and available before you can use them.
This includes EventBus and various other specific Olive services.

## Console Apps

To activate the Olive context in a console app add a nuget reference to `Olive`, `Olive.Console` and any other required Olive services. Then replace your Program startup file with the following:

```csharp
class Program
{
    static Task Main(string[] args) => Olive.Console.Application.Start<Startup>(args);
}

class Startup : Olive.Console.Startup
{
    public Startup(IConfiguration config) : base(config)
    {
        // config.LoadAwsDevIdentity("", "");
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        // ... any specific configurations will go here.

        // Using AWS services?
        // config.LoadAwsDevIdentity("...", "...", Amazon.RegionEndpoint.EUWest1, loadSecrets: false);

        // Using SQS?
        // services.AddAwsEventBus();

        // Using database?
        // services.AddDataAccess(x => x.SqlServer());
    }

    protected override Task Run()
    {
        Console.WriteLine("Hello!");
        return Task.CompletedTask;
    }
}
```
