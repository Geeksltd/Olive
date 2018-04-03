# Push Notifications

`Olive.PushNotification` exposes methods to help you send push notifications to ios, android and windows devices.

## Configuration

You need to setup push notification configuration in `appsettings.json`.

### configuration sample

```json
"PushNotification":
{
    "Apple":
    {
        "CertificateFile":"Path/To/Certificate.pfx",
        "Environment": "Production"
    },
    "Google":
    {
        "SenderId":"SomeID",
        "AuthToken":"Some Auth Token"
    },
    "Windows":
    {
        "PackageName":"Some Package name",
        "PackageSID":"Some Package ID",
        "ClientSecret":"Super Secret String"
    }

}
```

> **Note :** `Environment` of Apple can be either `Production` or `Sandbox`.

## Sending notification

`Olive.PushNotifications` exposes `IUserDevice` interface that helps you define push notifications.

```csharp
public interface IUserDevice
{
    /// <summary>
    /// IOS, Android or Windows.
    /// </summary>
    string DeviceType { get; }

    /// <summary>
    ///  Push notification token registered with the platform service.
    /// </summary>
    string PushNotificationToken { get; }
}
```

Then you need to call `Send(string messageTitle, string messageBody, IEnumerable<IUserDevice> devices)` method to push your notifications.