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

Now you should write your custom SMS provider, this class should inherit from `ISmsDispatcher`. 
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
Now you should open `appsettings.json` and add this section:
```json
"SMS": {
    "SenderType": "Olive.SMS.Tests.GeeksSmsDispatcher, Olive.SMS.Tests"
 }
```
> **Notice**: Please notice you should provide *fully qualified assembly name* here.

Open `Startup.cs` file and in the `ConfigureServices(...)` method add `services.AddSms();`. it should be something like this:
```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    services.AddSms();
}
```

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
        Date = LocalTime.Now,
		SenderName = "Geeks ltd",
		Text = "Thanks for using our service",
		To = "0000000"
    };
	
	await SmsService.Send(smsMessage);
}
```

As you can see we have injected `ISmsService` and then sent SMS directroy to the recipient.

> **Notice**: in this example, we have sent email instantly after saving to the database, but if you just save the email message in the database Olive will send emails on a regular basis that is configurable in the `TaskManager` class.