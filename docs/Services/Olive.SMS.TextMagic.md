# Olive.SMS.TextMagic

This document provides an overview and usage examples for the public classes and methods in the `Olive.SMS.TextMagic` namespace. It enables sending SMS messages using the TextMagic REST API. A configuration section details the required settings in an `appsettings.json` file.

---

## Table of Contents

1. [SmsDispatcher](#smsdispatcher)
   - [Overview](#smsdispatcher-overview)
   - [Methods](#smsdispatcher-methods)
2. [IServiceCollectionExtension](#iservicecollectionextension)
   - [Overview](#iservicecollectionextension-overview)
   - [Methods](#iservicecollectionextension-methods)
3. [Configuration](#configuration)

---

## SmsDispatcher

### SmsDispatcher Overview

The `SmsDispatcher` class implements `Olive.SMS.ISmsDispatcher` to send SMS messages via the TextMagic REST API, utilizing the `TextmagicRest` library for communication.

### SmsDispatcher Methods

- **`Dispatch(ISmsMessage sms)`**
  - Asynchronously sends an SMS message using the TextMagic API.
  - **Usage Example:**
    ```csharp
    var dispatcher = new SmsDispatcher();
    var sms = new SmsMessage { To = "+1234567890", Text = "Hello from TextMagic!" }; // Assuming SmsMessage implements ISmsMessage
    await dispatcher.Dispatch(sms);
    Console.WriteLine("SMS sent successfully");
    ```

---

## IServiceCollectionExtension

### IServiceCollectionExtension Overview

The `IServiceCollectionExtension` static class provides an extension method to register the TextMagic SMS dispatcher in an ASP.NET Core dependency injection container.

### IServiceCollectionExtension Methods

- **`AddTextMagic(this IServiceCollection @this)`**
  - Registers `SmsDispatcher` as the `ISmsDispatcher` in the service collection.
  - **Usage Example:**
    ```csharp 
    services.AddTextMagic(); 
    ```

---

## Configuration

The `SmsDispatcher` class requires specific configuration settings stored in an `appsettings.json` file with a JSON structure. Below are the required settings:

### Required Settings
- **`Sms:TextMagic:Username`**
  - The username for authenticating with the TextMagic API.
- **`Sms:TextMagic:Key`**
  - The API key (token) for authenticating with the TextMagic API.

### Full `appsettings.json` Example
```json
{
  "Sms": {
    "TextMagic": {
      "Username": "your-username",
      "Key": "your-api-key"
    }
  }
}
```

### Notes
- Both `Sms:TextMagic:Username` and `Sms:TextMagic:Key` must be provided in the configuration; otherwise, a `Config.GetOrThrow` exception will be thrown during instantiation.