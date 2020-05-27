# Olive.Email

This library will help you to send emails from your application and you can manage the history of your emails by persisting them in the database.

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

Olive exposes `IEmailMessage` interface under `Olive.Email` namespace, which enables you to implement email sending functionality. In order to send an email from your project using Olive, you must have an entity which implements this interface and you must have a database table having columns as per the properties in this interface. In the following code we have created an M# entity named `EmailMessage` that this class will implement `IEmailMessage` as shown below:

```csharp
using MSharp;

namespace Domain
{
    public class EmailMessage : EntityType
    {
        public EmailMessage()
        {
            SoftDelete();

            BigString("Body").Lines(5).Mandatory();
            String("From address");
            String("From name");
            String("Reply to address");
            String("Reply to name");
            String("Subject");
            String("To");
            BigString("Attachments");
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

Incidentally, you can add your custom logic here too. These configurations are enough for sending email, but if you want to create a custom template for your email content you can create another M# entity in your **#Model** project and inherit from `IEmailTemplate`. In the code below we have created an entity named `EmailTemplate`:

```csharp
using MSharp;

namespace Domain
{
    public class EmailTemplate : EntityType
    {
        public EmailTemplate()
        {
            InstanceAccessors("RegistrationConfirmationEmail");

			DefaultToString = String("Key").Mandatory().Unique();
            String("Subject").Mandatory();
            BigString("Body", 10).Mandatory();
            String("Mandatory placeholders");
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

To send an email you should simply populate `EmailMessage` class and inject `IEmailOutbox` in your class constructor or use `Context.Current.GetService<Olive.Email.IEmailOutbox>();` if you want to have property injection and call `.Send(...)`.

Here we have used class constructor injection as shown below:

```csharp
using Olive.Email;

namespace Domain
{
    public partial class ActionRequest
    {
        readonly IEmailOutbox EmailOutbox;

        public ActionRequest(IEmailOutbox emailOutbox)
        {
            this.EmailOutbox = emailOutbox;
        }

		[...]
    }
}
```

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
	await EmailOutbox.Send(emailMessage);
}
```
As you can see we have injected `IEmailOutbox` after that sent email message according to defined templated.
> **Notice** In case `IEmailOutbox` fails to send the email, It will save the email to the database and retry it using `TaskSchedular's` configured time unless retry count is greater than `MaxRetries` configured in `appsettings.json`
### Scheduling Email
IEmailMessage contains a property `SendableDate` which is used by `IEmailDispatcher` to identify when to send the email. By default, It's value is set to `LocalTime.Now` which means this email will be sent instantly. In case, you want to schedule the email for some other time, You can provide the value for any specific Date/Time.
```csharp
async void ScheduleRegistrationConfirmationEmail()
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
        SendableDate = LocalTime.Now.AddHours(1)
    };
	await EmailOutbox.Send(emailMessage);
}
```
### Email Dispatcher
`IEmailOutbox` uses `IEmailDispatcher` to send an email. Olive provides default implementation for `IEmailDispatcher` called `EmailDispatcher` using `SmtpClient`. If you want to provide some custom dispatch mechanism like SendGrid api, You can implement `IEmailDispatcher` and register it with dependency injection container using `Replace` method.
```csharp
using Olive.Email;

namespace Domain
{
    public partial class SendGridDispatcher : IEmailDispatcher
    {
        public Task Dispatch(MailMessage mail)
        {
            //dispatch implementation
        }
    }
```
Finally, You have to register `SendGridDispatcher` with dependency injection container.
```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    services.AddEmail();
    services.Replace<IEmailDispatcher, SendGridDispatcher>(ServiceLifetime.Singleton);
}
```
> **Notice** Always register your custom implementation after calling `services.AddEmail();` otherwise, It will be overrided with olive's default `EmailDispatcher`

## Recieve emails and check the email sending failures

To read the new emails you need to add `Olive.Email.Imap` NuGet Package. Then in you `Startup` add the IMAP service to your service collection like the following example. And you will have access to `IImapService` in your project.

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    base.ConfigureServices(services);
    services.AddImapService();
}
```

And if you need to check for the failure of the emails you sent inherit the `BaseEmailFailureService` and instead of `AddImapService` use `AddEmailFailureService` to add the services.