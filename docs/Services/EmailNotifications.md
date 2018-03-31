# Sending Emails

This tutorial explains how to implement email sending functionality using Olive framework. We will learn about the configuration required and the Interface provided in Olive framework.

## IEmailMessage Interface

Olive Framework exposes this interface under `Olive.Email` namespace, which enables you to implement email sending functionality. In order to send an email in your website using Olive framework, you must have an entity which implements this interface and you must have a database table having columns as per the properties in this interface. The code below shows the definition of `IEmailMessage` interface:

```csharp
public interface IEmailMessage : IEntity
    {
        /// <summary>
        /// Gets or sets the body of this email.
        /// </summary>
        string Body { get; set; }

        /// <summary>Gets or sets the Date this email becomes sendable.</summary>
        DateTime SendableDate { get; set; }

        /// <summary>Gets or sets whether this email is HTML.</summary>
        bool Html { get; set; }

        /// <summary>Gets or sets the From Address of this email.</summary>
        string FromAddress { get; set; }

        /// <summary>Gets or sets the From Name for this email.</summary>
        string FromName { get; set; }

        /// <summary>Gets or sets the ReplyTo Address of this email.</summary>
        string ReplyToAddress { get; set; }

        /// <summary>Gets or sets the ReplyTo Name for this email.</summary>
        string ReplyToName { get; set; }

        /// <summary>
        /// Gets or sets the Subject of this email.
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// Gets or sets the recipient of this email.
        /// </summary>
        string To { get; set; }

        /// <summary>
        /// Gets or sets the Attachments information for this email.
        /// </summary>
        string Attachments { get; set; }

        /// <summary>
        /// Gets or sets the Bcc recipients of this email.
        /// </summary>
        string Bcc { get; set; }

        /// <summary>
        /// Gets or sets the Bcc recipients of this email.
        /// </summary>
        string Cc { get; set; }

        /// <summary>
        /// Gets or sets the number of times sending this email has been tried.
        /// </summary>
        int Retries { get; set; }

        /// <summary>
        /// Gets or sets the VCalendar View of this email.
        /// </summary>
        string VCalendarView { get; set; }

        // -------------------------- Delivery settings override --------------------------

        /// <summary>
        /// Gets or sets whether SSL is enabled. If not set, the default config value will be used.
        /// </summary>
        bool? EnableSsl { get; set; }

        /// <summary>
        /// Gets or sets the Username to use for sending this email.
        /// If not set, the default config value will be used.
        /// </summary>
        string Username { get; set; }

        /// <summary>
        /// Gets or sets the Password to use for sending this email.
        /// If not set, the default config value will be used.
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Gets or sets the Smtp host address to use for sending this email.
        /// If not set, the default config value will be used.
        /// </summary>
        string SmtpHost { get; set; }

        /// <summary>
        /// Gets or sets the Smtp port to use for sending this email.
        /// If not set, the default config value will be used. 
        /// </summary>
        int? SmtpPort { get; set; }
    }
```

## Implementing Interface

In order to implement the interface we have created a new entity in our *"HelloWord"* project, which has all the properties defined in the IEmailMessage interface (For more information on creating entities in M#, please read [M# tutorial Entity, Page, Module](http://learn.msharp.co.uk/#/Basics/Concepts)).

```csharp
//This is an M# entity in #Model
using MSharp;

namespace Domain
{
    public class EmailMessage : EntityType
    {
        public EmailMessage()
        {
            SoftDelete();
            String("Body").Lines(5);
            DateTime("Date").Mandatory().Default("C#:LocalTime.Now");
            Bool("Enable Ssl").Mandatory();
            Bool("Html").Mandatory();
            String("Sender address").Max(200);
            String("Sender name").Max(200);
            String("Subject").Max(500).Mandatory();
            String("To").Max(500);
            String("Attachments").Max(500);
            String("Bcc");
            String("Cc");
            Int("Retries").Mandatory();
            String("VCalendar view");
            String("Username").Max(200);
            String("Password").Max(200).Accepts(TextPattern.Password);
            String("Smtp host").Max(200);
            Int("Smtp port");
            String("Category").Max(200);
        }
    }
}
```

![image](https://user-images.githubusercontent.com/22152065/38138323-8d2cdcde-343f-11e8-93cf-c4eab09ed33b.png)

> **Note :** The screenshot above shows `EmailMessage` entity created and a database table generated by M#, marked as soft delete, because we want to keep the log of what we will be sending in emails ([M# Entity tutorial](http://learn.msharp.co.uk/#/Domain/UnderstandingEntityTypes) explains soft deleting in more detail).

```csharp
partial class EmailMessage : IEmailMessage
{

}
```

The code above demonstrates the partial logic class of our EmailMessage entity, which implements the `IEmailMessage` interface (For details on creating business logic please read [M# tutorial Partial Classes and Business Logic](http://learn.msharp.co.uk/#/Domain/PartialClass)).

## Email Configuration

As we have successfully implemented `IEmailMessage` interface, now we need to look at some configuration in our `appsettings.json` file. The configuration required to send an email is generated by M# when a project is created, but we will take a look at these settings to know which settings are used for what purpose.

The "App Settings" shown below are generated by M# in `appsettings.json` file to support the email sending functionality

```json
"Email":
    {
        "MaximumRetries":4,
        "EnableSsl":true,
        "SmtpPort":3434,
        "SmtpHost":"",
        "Username": "" ,
        "Password" : "",
        "Permitted":
        {
            "Domains":"geeks.ltd.uk|uat.co",
            "Addresses":"NoReply@for-test.net"
        }
    }
```

## Sending an Email

The codes and explanation above was all about setting up `IEmailMessage` interface and configuration required. Now to send an email in M# we do not have to do anything complex. Simply populate the `IEmailMessage` instance with required details and call `Database.Save()` method to save the instance in database.

```csharp
partial class Employee
{
    protected override void OnSaved(SaveEventArgs e)
    {
        base.OnSaved(e);

        //Sends Registration Conformation Email
        if (e.Mode == SaveMode.Insert)
            SendRegistrationConformationEmail();
    }
    private void SendRegistrationConformationEmail()
    {
        Database.Save(new EmailMessage()
        {
            To = this.Email,
            Subject = "Hello Registration!",
            Body = "Thank you for registration"
        });
    }
}
```

The screenshot above demonstrates that we have implemented a new method in our `Employee` class, which populates a new instance of `IEmailMessage` and simply saves it in database. This method is called in **OnSaved** method of `Employee` class, which is raised just after a new *employee* is saved in database.

M# automatic process **SendEmailMessages** then sends this newly inserted `EmailMessage` record using `EmailService` class (For more details on SendEmailMessages process, please read [M# tutorial Automatic Tasks in chapter 14](http://learn.msharp.co.uk/#/Tutorials/14/README)).

## Email Service Class

`EmailService` is a static class provided under `Olive.Email` namespace. This class encapsulates the functionality to send an email using the configuration explained above in this tutorial. This class exposes methods to send emails by requiring `IEmailMessage` type argument. The screenshots below show the declaration of the methods to send an email.

> **Note :** `SendAll()` method overloads retrieve email records from **EmailMessage** database table.

```csharp
/// <summary>Tries to sends all emails.</summary>
 public static async Task SendAll(TimeSpan? delayPerSend = null)

/// <summary>
/// Will try to send the specified email and returns true for successful sending.
/// </summary>
public static async Task<bool> Send(IEmailMessage mailItem)

static async Task<bool> SendViaSmtp(IEmailMessage mailItem, MailMessage mail)

```

We often need to perform custom action before, or just after sending an email and also when an error occurs while sending an email. `EmailService` raises events for each action, which can be wired to perform custom logic. Shown below are the events and their description

```csharp
public static readonly AsyncEvent<EmailSendingEventArgs> Sending = new AsyncEvent<EmailSendingEventArgs>();

/// <summary>
/// Occurs when the smtp mail message for this email is sent. Sender is the IEmailQueueItem instance that was sent.
/// </summary>
public static readonly AsyncEvent<EmailSendingEventArgs> Sent = new AsyncEvent<EmailSendingEventArgs>();

/// <summary>
/// Occurs when an exception happens when sending an email. Sender parameter will be the IEmailQueueItem instance that couldn't be sent.
/// </summary>
public static readonly AsyncEvent<EmailSendingEventArgs> SendError = new AsyncEvent<EmailSendingEventArgs>();
```

> **Note :** Checkout [M# Email tutorial](http://learn.msharp.co.uk/#/Tutorials/14/README) to have a deep look email notifications.