# Olive.SMS

This library will help you to send SMS with your customized SMS providers.

## Getting started

First, you need to add the [Olive.SMS](https://www.nuget.org/packages/Olive.SMS/) NuGet package : `Install-Package Olive.SMS`.
After adding nuget package you should write your custom SMS provider, this class should inherit from `ISmsDispatcher`. 
For example, here we create a class named `GeeksSmsDispatcher`:
```csharp
namespace Olive.SMS.Tests
{
    public class GeeksSmsDispatcher : ISmsDispatcher
    {
        public Task Dispatch(ISmsMessage sms)
        {
            //send sms
        }
    }
}
```

Open `Startup.cs` file and in the `ConfigureServices(...)` method add `services.AddSms();`. it should be something like this:
```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    services.AddSms();
}
```

### Sending SMS 

First, you should implement `ISmsMessage` interface:

```csharp
public class SmsMessage : GuidEntity, ISmsMessage
{
    public DateTime Date { get; set; }
    public DateTime? DateSent { get; set; }
    public string SenderName { get; set; }
    public string Text { get; set; }
    public string To { get; set; }
    public int Retries { get; set; }      
    public DeliveryStatus Status {get; set;} 
}
```

You can send SMS easily by just injecting 'ISmsService' interface in the class constructor or use `Context.Current.GetService<ISmsService>();` and use `Send()` method to send an SMS.