# Olive.Aws.Ses

## Overview
The `Olive.Aws.Ses` provides functionality for sending emails using Amazon Simple Email Service (SES). It converts `MailMessage` objects into MIME messages and dispatches them using AWS SES.

## Configuration
Ensure you have the necessary AWS credentials and SES permissions configured. 

## Class: `AwsSesEmailDispatcher`

### `Dispatch`
```csharp
public async Task Dispatch(MailMessage mail, IEmailMessage iEmailMessage)
```
- **Summary**: Sends an email using AWS SES.
- **Usage**:
```csharp
IEmailMessage iEmailMessage = ??

var mail = new MailMessage("from@example.com", "to@example.com")
{
    Subject = "Test Email",
    Body = "Hello, this is a test email."
};
await new AwsSesEmailDispatcher().Dispatch(mail, iEmailMessage);
```
- **Notes**:
  - Converts the `MailMessage` to a `MimeMessage`.
  - Calls AWS SES API to send the email.
  - Supports calendar event attachments (`VCalendarView`).
  - Adds file attachments to the email.
  - Throws an exception if the email fails to send.  


## Class: `Extensions`

### `ToRawMessage`
```csharp
internal static RawMessage ToRawMessage(this MimeMessage message)
```
- **Summary**: Converts a `MimeMessage` into an AWS SES-compatible `RawMessage`.
- **Usage**:
```csharp
var rawMessage = mimeMessage.ToRawMessage();
```

## Class: `IServiceCollectionExtension`

### `AddAwsSesProvider`
```csharp
public static IServiceCollection AddAwsSesProvider(this IServiceCollection services)
```
- **Summary**: Registers `AwsSesEmailDispatcher` as the default email dispatcher in dependency injection.
- **Usage**:
```csharp
services.AddAwsSesProvider();
```

## Full Example
```csharp  
var dispatcher = Context.Current.GetService<IEmailDispatcher>();

IEmailMessage iEmailMessage = ??
var mail = new MailMessage("from@example.com", "to@example.com")
{
    Subject = "Hello World",
    Body = "This is a test email."
};

await dispatcher.Dispatch(mail, iEmailMessage);
```

## Conclusion
The `Olive.Aws.Ses` provides a simple and effective way to send emails using AWS SES. It supports attachments, calendar invites, and integrates with dependency injection for easy use in applications.
