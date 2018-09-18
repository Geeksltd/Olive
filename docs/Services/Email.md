# Olive.Email

This library will help you to send emails from your application and you can manage history of your emails.

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

Olive exposes an interface under `Olive.Email` namespace, which enables you to implement email sending functionality. In order to send an email in your project using Olive, you must have an entity which implements this interface and you must have a database table having columns as per the properties in this interface. In the folowing code we have created a M# entity named `EmailMessage`:

```csharp
using MSharp;

namespace Domain
{
    public class EmailMessage : EntityType
    {
        public EmailMessage()
        {
            SoftDelete();

            String("Body").Max(5000).Lines(5);
            String("From address");
            String("From name");
            String("Reply to address");
            String("Reply to name");
            String("Subject").Max(500);
            String("To").Max(500);
            String("Attachments").Max(500);
            String("Bcc").Max(2000);
            String("Cc").Max(2000);
            String("VCalendarView");
            Int("Retries").Mandatory();
            DateTime("Sendable date").Default("c#:LocalTime.Now").Mandatory();
            Bool("Html").Mandatory();
        }
    }
}
```
By building your `#Model` project, M# will generate related code and in the **Logic** folder you must inherit this class from `IEmailMessage` interface:

```csharp
using Olive.Email;

namespace Domain
{
    public partial class EmailMessage : IEmailMessage
    {
    }
}
```

This configuration is enough for sending email, but if you want to create custom template for your email content you should create another M# entity in your **#Model** project and inherit from `IEmailTemplate`. In the code below we have create an entity named `EmailTemplate`:

```csharp
using MSharp;

namespace Domain
{
    public class EmailTemplate : EntityType
    {
        public EmailTemplate()
        {
            InstanceAccessors("RegistrationConfirmationEmail");

            String("Body").Max(5000).Lines(10);
            String("Key").Mandatory().Unique();
            String("Mandatory placeholders").Mandatory();
            String("Subject").Mandatory();
        }
    }
}
```
After building **#Model** project you must inherit this class from `IEmailTemplate` as shown below:

```csharp
using Olive.Email;

namespace Domain
{
    public partial class EmailTemplate : IEmailTemplate
    {
    }
}
```

You have now completed the initial setup of the `Olive.Email` service.

### Sending Email 

To send an email you should simply populate `EmailMessage` class and inject `IEmailOutbox` in your class constructor or use `Context.Current.GetService<Olive.Email.IEmailOutbox>();` and call `.Send(...)` method:

```csharp
async void SendRegistrationConfirmationEmail()
{
    var template = EmailTemplate.RegistrationConfirmationEmail;

    var placeHolderValues = new
    {
        LastName = this.LastName,
        Email = this.Email,
        Password = this.Password
    };

    var emailMessage = new EmailMessage
    {
        Subject = template.MergeSubject(placeHolderValues),
        To = this.AssigneeEmail,
        Body = template.MergeBody(placeHolderValues)
    };

	await Database.Save(emailMessage);

    await emailOutbox.Send(emailMessage);
}
```

As you can see we have injected `IEmailOutbox`, first saved email message and then send email message according to defined templated.