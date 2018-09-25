# Olive.SMS

This library will help you to send and persist SMS with your customized SMS providers.

## Getting started

First, you need to add the [Olive.SMS](https://www.nuget.org/packages/Olive.SMS/) NuGet package : `Install-Package Olive.SMS`.

Olive exposes `ISmsMessage` interface under `Olive.SMS` namespace, which enables you to implement SMS sending functionality. In order to send an SMS from your project using Olive, you must have an entity which implements this interface and you must have a database table having columns as per the properties in this interface. In the following code we have created an M# entity named `SmsMessage` that this class will implement `ISmsMessage` as shown below:

```csharp
using MSharp;

namespace Domain
{
    public class SmsMessage : EntityType
    {
        public SmsMessage()
        {
            SoftDelete();

            String("Text").Max(5000).Lines(5);
            String("SenderName");
            String("To");
            Int("Retries").Mandatory();
            DateTime("Date").Default("c#:LocalTime.Now").Mandatory();
            DateTime("DateSent");
        }
    }
}
```
By building your `#Model` project, M# will generate related code and in the **Logic** folder you must inherit this class from `ISmsMessage` interface:

```csharp
using Olive.Email;

namespace Domain
{
    public partial class SmsMessage : ISmsMessage
    {
    }
}
```

By default, Olive provides `NullSmsDispatcher` which does not dispatch the messages but marks them sent. You should implement your custom SMS dispatcher and register it with dependency injection container, this class should inherit from `ISmsDispatcher`. 
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
    services.Replace<ISmsDispatcher, GeeksSmsDispatcher>(ServiceLifetime.Singleton);
}
```
> **Notice** Always register your custom implementation after calling `services.AddSms();` otherwise, It will be overrided with olive's default `NullSmsDispatcher`
### Sending SMS 

To send a SMS you should simply populate `SmsMessage` class and inject `ISmsService` in your class constructor or use `Context.Current.GetService<Olive.SMS.ISmsService>();` if you want to have property injection and call `.Send(...)` method as show below:

Here we have used class constructor injection as shown below:

```csharp
using Olive.SMS;

namespace Domain
{
    public partial class ActionRequest
    {
        readonly ISmsService SmsService;

        public ActionRequest(ISmsService smsService)
        {
            this.SmsService = smsService;
        }

		[...]
    }
}
```

```csharp
async void SendSMS()
{

    var smsMessage = new SmsMessage
    {
       SenderName = "Geeks ltd",
       Text = "Thanks for using our service",
       To = "0000000"
    };
	
    await SmsService.Send(smsMessage);
}
```

As you can see we have injected `ISmsService` and then sent SMS directroy to the recipient.
### Scheduling SMS
`ISmsMessage` contains a property `Date` which is used by `ISmsService` to identify when to send the Sms. By default, It's value is set to `LocalTime.Now` which means this sms will be sent instantly. In case, you want to schedule the Sms for some later time, You can provide the value for any specific Date/Time.
```csharp
async void SendSMS()
{

    var smsMessage = new SmsMessage
    {
       SenderName = "Geeks ltd",
       Text = "Thanks for using our service",
       To = "0000000",
       Date = LocalTime.Now.AddHours(1)
    };
	
    await SmsService.Send(smsMessage);
}
```