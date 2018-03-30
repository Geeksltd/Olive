# Sending SMS

In this tutorial we go through the implementation of sending an SMS using M# framework. We will take a look at the interfaces, classes and configuration required to set up SMS sending functionality.

## ISmsQueueItem

M# Framework exposes this interface under **Olive.Services** namespace, which must be implemented in order to send SMS. This interface defines the contents of the actual SMS and is implemented on an entity class, which is used to store SMS related information in database. The blocks of code below shows `ISmsQueueItem` definition.

```csharp
 public interface ISmsQueueItem : IEntity
    {
        /// <summary>
        /// Gets or sets the date this SMS should be sent.
        /// </summary>
        DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the date when this SMS was successfully sent.
        /// </summary>
        DateTime? DateSent { get; set; }

        /// <summary>
        /// Gets or sets the Sender Name.
        /// </summary>
        string SenderName { get; set; }

        /// <summary>
        /// Gets or sets the SMS text.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Gets or sets the SMS recipient number.
        /// </summary>
        string To { get; set; }

        /// <summary>
        /// Gets or sets the number of times sending this email has been tried.
        /// </summary>
        int Retries { get; set; }

    }
```

## ISmsQueueItem Implementation

You must create an entity in M# which implements `ISmsQueueItem` in business logic. This tutorial does not explain the steps to implement the `ISmsQueueItem` interface as it follows same steps we performed in tutorial [Sending Email]((Services/EmailNotifications.md)) for implementing `IEmailQueueItem`.

## ISMSSender Interface

Olive doesn’t provide default logic for sending an SMS but exposes `ISMSSender` interface, which defines only one method. This interface must be implemented and the method should implement the logic of sending an SMS. You should not write any exception handling logic in this method. Any exception in this method will be handled by Olive and will be logged. The blocks of code below shows `ISMSSender` definition:

```csharp
public interface ISMSSender
{
    /// <summary>
    /// Delivers the specified SMS message.
    /// The implementation of this method should not handle exceptions. Any exceptions will be logged by the engine.
    /// </summary>
    void Deliver(ISmsQueueItem sms);
}
```

## SMS Configuration

Olive looks for the two `appsettings.json` in the configuration file shown below:

```json
"SMS":
{
    "MaximumRetries":4,
    "SenderType":"App.SMSSender"
}
```

 `MaximumRetries` is an optional setting, which is used to determine the maximum number of reties Olive should perform to send an SMS. Olive reties 3 times by default when this setting is not provided.

 `SenderType` is a required setting, which is used to load the instance of type that implements `ISMSSender` interface. Olive throws exception if type is not defined or doesn’t implement `ISMSSender` interface.

## SmsService

Olive exposes a static class `SmsService` under `Olive.Services` which is responsible for sending SMS. You must define an *Automatic Task* in M#, which calls the appropriate method to send an SMS (please read [M# tutorial Automatic Task](http://learn.msharp.co.uk/#/Tutorials/18/README) for more details).

`SmsService` class exposes one public event handler which can be used to capture exceptions and two methods to send the SMS. The code below shows the definition of `SmsService` class.

```csharp
public static class SmsService
    {
        /// <summary>
        /// Occurs when an exception happens when sending an sms. Sender parameter will be the ISmsQueueItem instance that couldn't be sent.
        /// </summary>
        public static readonly AsyncEvent<SmsSendingEventArgs> SendError = new AsyncEvent<SmsSendingEventArgs>();

        /// <summary>
        /// Sends the specified SMS item.
        /// It will try several times to deliver the message. The number of retries can be specified in AppConfig of "SMS:MaximumRetries".
        /// If it is not declared in web.config, then 3 retires will be used.
        /// Note: The actual SMS Sender component must be implemented as a public type that implements ISMSSender interface.
        /// The assembly qualified name of that component, must be specified in AppConfig of "SMS:SenderType".
        /// </summary>
        public static async Task<bool> Send(ISmsQueueItem smsItem);

        public static async Task SendAll();
    }
```

>**Note :** You must implement `ISMSSender` in your project, which should contain the SMS sending logic. *Send* method implemented in `SmsService` doesn’t contain the logic to send SMS. It handles the `ISmsQueueItem` instance and invokes `ISMSSender` implantation to send an SMS and maintains the retries. `SendAll()` method tries to send all the `SMSQueueItems` pending in the database table not marked as sent.