# Olive.Email

This library will help you to send emails as easy as possible.

## Getting started

First, you need to add the [Olive.Email](https://www.nuget.org/packages/Olive.Email/) NuGet package : `Install-Package Olive.Email`.
Now you should add this section to `appsettings.json` file:
```json
"Email": {
    "From": {
      "Name": "My Company",
      "Address": "noreply@mycompany.com"
    },
    "Permitted": {
      "Domains": "mycompany.com",
      "Addresses": "..."
    },
    "ReplyTo": {
      "Name": "My Company",
      "Address": "support@mycompany.com"
    },
    "EnableSsl": "true",
    "SmtpPort": "587",
    "SmtpHost": "...",
    "Username": "...",
    "Password": "...",
    "MaxRetries": "4"
 }
```
Open `Startup.cs` file and in the `ConfigureServices(...)` method add `services.AddEmail();`. it should be something like this:
```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    services.AddEmail();
}
```

You have now completed the initial setup of the `Olive.Email` service.

### Sending Email 

Add `using Olive;` in top of your csharp file and implement `IEmailMessage` and `IEmailTemplate`. Here is a full example:
```csharp
public class EmailMessage : GuidEntity, IEmailMessage
{
    public string Body { get; set; }
    public DateTime SendableDate { get; set; }
    public bool Html { get; set; }
    public string FromAddress { get; set; }
    public string FromName { get; set; }
    public string ReplyToAddress { get; set; }
    public string ReplyToName { get; set; }
    public string Subject { get; set; }
    public string To { get; set; }
    public string Attachments { get; set; }
    public string Bcc { get; set; }
    public string Cc { get; set; }
    public int Retries { get; set; }
    public string VCalendarView { get; set; }    
}
```

```c#
public class MailTemplate : GuidEntity, IEmailTemplate
{
    public string Body { get; set; }
    public string Key { get; set; }
    public string MandatoryPlaceholders { get; set; }
    public string Subject { get; set; }    
}
```

You need to inject `IEmailMessage` interface into your class constructor or in some cases, you may need to inject it using `Context.Current.GetService<IEmailSender>();` and use `Send()` method to send email.