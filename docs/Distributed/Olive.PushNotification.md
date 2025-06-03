# Olive.PushNotification

This document provides an overview and usage examples for the public classes and methods in the `Olive.PushNotification` namespace. It enables sending push notifications to iOS, Android, and Windows devices using platform-specific brokers. A configuration section details the required settings in an `appsettings.json` file.

---

## Table of Contents

1. [PushNotificationService](#pushnotificationservice)
   - [Overview](#pushnotificationservice-overview)
   - [Methods](#pushnotificationservice-methods)
2. [PushNotificationExtensions](#pushnotificationextensions)
   - [Overview](#pushnotificationextensions-overview)
   - [Methods](#pushnotificationextensions-methods)
3. [IUserDevice](#iuserdevice)
   - [Overview](#iuserdevice-overview)
4. [ISubscriptionIdResolver](#isubscriptionidresolver)
   - [Overview](#isubscriptionidresolver-overview)
   - [Methods](#isubscriptionidresolver-methods)
5. [IPushNotificationService](#ipushnotificationservice)
   - [Overview](#ipushnotificationservice-overview)
   - [Methods](#ipushnotificationservice-methods)
6. [Configuration](#configuration)

---

## PushNotificationService

### PushNotificationService Overview

The `PushNotificationService` class implements `IPushNotificationService` to send push notifications and update badge counts across iOS, Android, and Windows devices using platform-specific brokers from PushSharp.

### PushNotificationService Methods

- **`Send(string messageTitle, string messageBody, IEnumerable<IUserDevice> devices)`**
  - Sends a push notification with a title and body to the specified devices.
  - **Usage Example:**
    ```csharp     
    var resolver = new NullSubscriptionIdResolver();
    var service = new PushNotificationService(logger, resolver);
    var devices = new List<IUserDevice>
    {
        new MyUserDevice { DeviceType = "iOS", PushNotificationToken = "ios-token" },
        new MyUserDevice { DeviceType = "Android", PushNotificationToken = "android-token" }
    };
    bool success = service.Send("Hello", "This is a test notification", devices);
    Console.WriteLine($"Notification sent: {success}");
    ```

- **`UpdateBadge(int badge, IEnumerable<IUserDevice> devices)`**
  - Updates the badge count on the specified devices (currently implemented for iOS only).
  - **Usage Example:**
    ```csharp
    
    var resolver = new NullSubscriptionIdResolver();
    var service = new PushNotificationService(logger, resolver);
    var devices = new List<IUserDevice>
    {
        new MyUserDevice { DeviceType = "iOS", PushNotificationToken = "ios-token" }
    };
    bool success = service.UpdateBadge(5, devices);
    Console.WriteLine($"Badge updated: {success}");
    ```

---

## PushNotificationExtensions

### PushNotificationExtensions Overview

The `PushNotificationExtensions` static class provides extension methods to register push notification services in an ASP.NET Core dependency injection container.

### PushNotificationExtensions Methods

- **`AddPushNotification(this IServiceCollection @this)`**
  - Registers `PushNotificationService` with a default `NullSubscriptionIdResolver`.
  - **Usage Example:**
    ```csharp
 
    services.AddPushNotification();
    
    var pushService = Context.Current.GetService<IPushNotificationService>();
    ```

- **`AddPushNotification<TResolver>(this IServiceCollection @this)`**
  - Registers `PushNotificationService` with a custom `ISubscriptionIdResolver` implementation.
  - **Usage Example:**
    ```csharp
    public class CustomResolver : ISubscriptionIdResolver
    {
        public Task ResolveExpiredSubscription(string oldId, string newId) => Task.CompletedTask;
    }
    
    services.AddPushNotification<CustomResolver>();
 
    var pushService = Context.Current.GetService<IPushNotificationService>();
    ```

---

## IUserDevice

### IUserDevice Overview

The `IUserDevice` interface defines the structure for a user device that can receive push notifications, specifying its type and token.

- **Usage Example:**
  ```csharp
  public class MyUserDevice : IUserDevice
  {
      public string DeviceType => "iOS";
      public string PushNotificationToken => "ios-device-token";
  }
  ```

---

## ISubscriptionIdResolver

### ISubscriptionIdResolver Overview

The `ISubscriptionIdResolver` interface defines a contract for resolving expired subscription IDs when push notifications fail due to token expiration.

### ISubscriptionIdResolver Methods

- **`ResolveExpiredSubscription(string oldSubscriptionId, string newSubscriptionId)`**
  - Resolves an expired subscription by updating or removing the old ID.
  - **Usage Example:**
    ```csharp
    public class CustomResolver : ISubscriptionIdResolver
    {
        public async Task ResolveExpiredSubscription(string oldId, string newId)
        {
            Console.WriteLine($"Replacing {oldId} with {newId ?? "null"}");
            await Task.CompletedTask;
        }
    }
    ```

---

## IPushNotificationService

### IPushNotificationService Overview

The `IPushNotificationService` interface defines the contract for sending push notifications and updating badge counts across supported platforms.

### IPushNotificationService Methods

- **`Send(string messageTitle, string messageBody, IEnumerable<IUserDevice> devices)`**
  - Sends a push notification to the specified devices.
  - **Usage Example:** See `PushNotificationService.Send` above.

- **`UpdateBadge(int badge, IEnumerable<IUserDevice> devices)`**
  - Updates the badge count on the specified devices.
  - **Usage Example:** See `PushNotificationService.UpdateBadge` above.

---

## Configuration

The `PushNotificationService` class requires specific configuration settings stored in an `appsettings.json` file with a JSON structure to initialize platform-specific brokers. Below are the settings for each platform:

### Apple (iOS)
- **`PushNotification:Apple:CertificateFile`** (Required)
  - Path to the `.p12` or `.pfx` certificate file for APNs.
- **`PushNotification:Apple:CertificatePassword`** (Required)
  - Password for the certificate file.
- **`PushNotification:Apple:Environment`** (Required)
  - APNs environment (`Production` or `Sandbox`).
- **Example:**
  ```json
  {
    "PushNotification": {
      "Apple": {
        "CertificateFile": "Certificates/apns-cert.p12",
        "CertificatePassword": "your-password",
        "Environment": "Production"
      }
    }
  }
  ```

### Google (Android)
- **`PushNotification:Google:SenderId`** (Required)
  - Sender ID from Google Cloud Messaging (FCM).
- **`PushNotification:Google:AuthToken`** (Required)
  - Server key (authentication token) from FCM.
- **Example:**
  ```json
  {
    "PushNotification": {
      "Google": {
        "SenderId": "your-sender-id",
        "AuthToken": "your-auth-token"
      }
    }
  }
  ```

### Windows
- **`PushNotification:Windows:PackageName`** (Required)
  - Package name from the Windows Dev Center.
- **`PushNotification:Windows:PackageSID`** (Required)
  - Security Identifier (SID) for the package.
- **`PushNotification:Windows:ClientSecret`** (Required)
  - Client secret from the Windows Dev Center.
- **Example:**
  ```json
  {
    "PushNotification": {
      "Windows": {
        "PackageName": "your-package-name",
        "PackageSID": "your-package-sid",
        "ClientSecret": "your-client-secret"
      }
    }
  }
  ```

### Full `appsettings.json` Example
```json
{
  "PushNotification": {
    "Apple": {
      "CertificateFile": "Certificates/apns-cert.p12",
      "CertificatePassword": "your-password",
      "Environment": "Production"
    },
    "Google": {
      "SenderId": "your-sender-id",
      "AuthToken": "your-auth-token"
    },
    "Windows": {
      "PackageName": "your-package-name",
      "PackageSID": "your-package-sid",
      "ClientSecret": "your-client-secret"
    }
  }
}
```

### Notes
- Each platform's broker is initialized only if its respective configuration is provided. If a configuration is missing (e.g., `CertificateFile` for Apple), that platform will be skipped.
