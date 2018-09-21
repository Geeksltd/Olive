# Olive.PushNotification

This library will help you to send Push Notifications to iOS (iPhone/iPad APNS), Android (C2DM and GCM - Google Cloud Message), Windows Phone, Windows 8, Amazon, Blackberry, and (soon) FirefoxOS devices!

## Getting started

First, you need to add the [Olive.PushNotification](https://www.nuget.org/packages/Olive.PushNotification/) NuGet package : `Install-Package Olive.PushNotification`.
Now you should add this section to `appsettings.json` file:
```json
"PushNotification": {
	"Apple": {
		"CertificateFile": "...",
		"Environment": "Sandbox",
		"CertificatePassword": "..."
	},
	"Google": {
		"SenderId": "...",
		"AuthToken": "..."
	},
	"Windows": {
		"PackageName": "...",
		"PackageSID": "...",
		"ClientSecret": "..."
	}
}
```
> **Note**: Please notice that depending on your development environment you may use **SandBox** or **Production**

Open `Startup.cs` file and in the `ConfigureServices(...)` method add `services.AddPushNotification();`. it should be something like this:
```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    services.AddPushNotification();
}
```

Olive exposes `IPushNotificationService` interface under `Olive.PushNotification` namespace, which enables you to implement push notification sending functionality. In order to send a push notification from your project using Olive, you must have an class which implements `IUserDevice` interface as shown below:

```csharp
public class iOSUserDevice : IUserDevice
{
    public string DeviceType => "iOS";

    public string PushNotificationToken => "Token";
}
```

It's better you add your class in **Domain** project under **Logic** folder.

### Sending Push Notification 

To send a push notification you should simply inject `IPushNotificationService` in your class constructor or use `Context.Current.GetService<Olive.PushNotification.IPushNotificationService>();` if you want to have property injection and call `.Send(...)`.

Here we have used class constructor injection as shown below:

```csharp
using Olive.PushNotification;

namespace Domain
{
    public partial class ActionRequest
    {
        readonly IPushNotificationService PushNotificationService;

        public ActionRequest(IPushNotificationService pushNotificationService)
        {
            this.PushNotificationService = pushNotificationService;
        }

		[...]
    }
}
```

```csharp
async void SendPushNotificationService()
{
    var items = new List<iOSUserDevice>
            {
                new iOSUserDevice()
            };

    PushNotificationService.Send("Title", "Geeks push notification", items);
}
```