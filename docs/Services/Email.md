# Olive.Email

This library will help you to send emails as easy as possible.

## Getting started

First, you need to add the [Olive.Email](https://www.nuget.org/packages/Olive.Email/) NuGet package : `Install-Package Olive.Email`.
Now you should add these section to `appsettings.json` file:
```json
"Email": {
    "From": {
      "Name": "From Geeks",
      "Address": "info@geeks.ltd.uk"
    },
    "Permitted": {
      "Domains": "geeks.ltd.uk",
      "Addresses": "info@geeks.ltd.uk"
    },
    "ReplyTo": {
      "Name": "To Geeks",
      "Address": "info@geeks.ltd.uk"
    },
    "EnableSsl": "false",
    "SmtpPort": "25",
    "SmtpHost": "127.0.0.1",
    "Username": "username",
    "Password": "password",
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
``csharp
public class EmailMessage : IEmailMessage
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
    public bool? EnableSsl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public bool IsNew => true;
    public IEntity Clone() => this;
    public int CompareTo(object obj) => 0;
    public object GetId() => this;
    public void InvalidateCachedReferences()
    {
    }
    public Task Validate() => Task.CompletedTask;
}
```

```c#
public class MailTemplate : IEmailTemplate
{
    public string Body { get; set; }
    public string Key { get; set; }
    public string MandatoryPlaceholders { get; set; }
    public string Subject { get; set; }
    public bool IsNew => true;
    public IEntity Clone() => this;
    public int CompareTo(object obj) => 0;
    public object GetId() => this;
    public void InvalidateCachedReferences()
    {
    }
    public Task Validate() => Task.CompletedTask;
}
```

You need to inject `IEmailMessage` interface into your class constructor or in some cases, you may need to inject it using `Context.Current.GetService<IEmailSender>();` and use `Send()` method to send email.