# Olive.Sms.Aws

This document provides an overview and usage examples for the public classes and methods in the `Olive.SMS` namespace related to sending SMS messages via Amazon Simple Notification Service (SNS). It integrates with AWS SNS to dispatch SMS messages, supporting optional sender ID and origination number configurations. A configuration section details the optional settings in an `appsettings.json` file.

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

The `SmsDispatcher` class implements `ISmsDispatcher` and `IDisposable` to send SMS messages using Amazon SNS. It uses the `AmazonSimpleNotificationServiceClient` to publish messages and supports optional attributes like sender ID and origination number.

### SmsDispatcher Methods

- **`Dispatch(ISmsMessage sms)`**
  - Asynchronously sends an SMS message via AWS SNS with configurable attributes.
  - **Usage Example:**
    ```csharp  
    var dispatcher = Context.Current.GetService<ISmsDispatcher>();
    var sms = new SmsMessage { To = "+1234567890", Text = "Hello from AWS SNS!" }; // Assuming SmsMessage implements ISmsMessage
    await dispatcher.Dispatch(sms);
    Console.WriteLine("SMS sent successfully");
    ``` 

---

## IServiceCollectionExtension

### IServiceCollectionExtension Overview

The `IServiceCollectionExtension` static class provides an extension method to register the AWS SNS SMS dispatcher in an ASP.NET Core dependency injection container.

### IServiceCollectionExtension Methods

- **`AddAwsSms(this IServiceCollection @this)`**
  - Registers `SmsDispatcher` as the `ISmsDispatcher` in the service collection.
  - **Usage Example:**
    ```csharp     
    services.AddAwsSms();
    ```

---

## Configuration

The `SmsDispatcher` class supports optional configuration settings stored in an `appsettings.json` file with a JSON structure. These settings enhance the SMS sending process but are not strictly required. Below are the optional settings:

### Optional Settings
- **`Aws:Sns:SenderId`**
  - The sender ID to display on the recipient's device (e.g., a brand name). Overrides `ISmsMessage.SenderName` if specified in the message.
- **`Aws:Sns:OriginationNumber`**
  - The phone number to use as the origination number for the SMS (must be an AWS SNS-supported number).

### Full `appsettings.json` Example
```json
{
  "Aws": {
    "Sns": {
      "SenderId": "MyApp",
      "OriginationNumber": "+12025550123"
    }
  }
}
```

### Notes
- If `SenderId` or `OriginationNumber` are not provided in `appsettings.json`, the dispatcher will use the `SenderName` from the `ISmsMessage` (if available) or omit these attributes.
- The SMS type is hardcoded to `"Transactional"` for immediate delivery, suitable for critical notifications.
