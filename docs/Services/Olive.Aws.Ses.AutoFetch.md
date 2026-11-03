# Olive.Aws.Ses.AutoFetch

## Overview
The `Olive.Aws.Ses.AutoFetch` provides a mechanism for automatically fetching emails from AWS Simple Email Service (SES) stored in an S3 bucket. It supports processing emails, extracting attachments, and saving them in a structured database.

## Setting Up the Bucket
Refer to [AWS SES Documentation](https://aws.amazon.com/premiumsupport/knowledge-center/ses-receive-inbound-emails/) for setting up the bucket.

In `Startup.cs`, initialize the fetching engine:

```csharp
using Olive.Aws.Ses.AutoFetch;

public override void Configure(IApplicationBuilder app)
{
    ....
    // Option 1: Use the built-in default type of AutoFetch.MailMessage.
    await Mailbox.Watch("s3 bucket name where SES is pointing to");

    // Option 2: Use your own type, which should implement AutoFetch.IMailMessage & AutoFetch.IMailMessageAttachment.
    await Mailbox.Watch<Domain.MailMessage,Domain.MailMessageAttachment>("s3 bucket name where SES is pointing to");
}
```

### Note
If **Option 1** is used, upon application startup, the following database tables will be automatically created:

```sql
CREATE TABLE [MailMessage] ...
CREATE TABLE [MailMessageAttachments] ...
```

If **Option 2** is used, add these entities:

```csharp
class MailMessage : EntityType
{         
    public MailMessage()
    {
        Implements("Olive.Aws.Ses.AutoFetch.IMailMessage");

        String("From");
        BigString("To");
        String("Bcc");
        String("Cc");
        String("Subject");
        BigString("Body");
        String("Sender");
        DateTime("Date").Mandatory();
        DateTime("DateDownloaded").Mandatory();
        String("Bucket");
        String("MessageId");
        String("ReplyTo");

        InverseAssociate<MailMessageAttachment>("Attachments", "MailMessage");
    }
}

class MailMessageAttachment : EntityType
{         
    public MailMessageAttachment()
    {
        Implements("Olive.Aws.Ses.AutoFetch.IMailMessageAttachment");

        Associate<MailMessage>("MailMessage");
        OpenFile("Attachment");
    }
}
```

## Execution
Create a background process to call:

```csharp
await Olive.Aws.Ses.AutoFetch.Mailbox.FetchAll();
```

## Convert Old Attachments to Physical Files

### Steps
1. Add the following methods to the `MailMessage` logic class:

```csharp
public static async Task ConvertOldAttachmentsToPhysicalFile()
{
    var messages = await Database.Of<MailMessage>()
        .Where(x => x.Attachments.HasValue() && x.Attachments != "[]")
        .Top(10).GetList();

    var newAttachments = new List<MailMessageAttachment>();
    var updatedMessages = new List<MailMessage>();

    foreach (var message in messages)
    {
        var attachments = message.GetAttachments();

        if (attachments.HasAny())
        {
            foreach (var (FileName, Data) in attachments)
            {
                newAttachments.Add(new MailMessageAttachment
                {
                    MailMessageId = message.ID,
                    Attachment = new Olive.Entities.Blob(Data, FileName)
                });
            }

            var cloneMessage = message.Clone();
            cloneMessage.Attachments = null;
            updatedMessages.Add(cloneMessage);
        }
    }

    using (var scope = Database.CreateTransactionScope())
    {
        await Database.Save(newAttachments.ToArray());
        await Database.BulkUpdate(updatedMessages.ToArray());
        scope.Complete();
    }
}

(string FileName, byte[] Data)[] GetAttachments()
{
    return JArray.Parse(Attachments)
            .Select(x => (x["FileName"].ToString(), x["Base64"].ToString().ToBytesFromBase64()))
            .ToArray();
}
```

2. Create a background process to call:

```csharp
await Domain.MailMessage.ConvertOldAttachmentsToPhysicalFile();
```

## Class: `Mailbox`

### `Watch`
```csharp
public static async Task Watch(string emailS3Bucket)
```
- **Summary**: Monitors the specified S3 bucket for incoming emails and initializes database providers for mail storage.
- **Usage**:

```csharp
await Mailbox.Watch("my-email-bucket");
```

### `FetchAll`
```csharp
public static Task FetchAll()
```
- **Summary**: Fetches all emails from the monitored S3 bucket and stores them in the database.
- **Usage**:

```csharp
await Mailbox.FetchAll();
```

## Class: `DatabaseTableService`

### `EnsureDatabaseTable`
Ensures that the necessary database tables (`MailMessages`, `MailMessageAttachments`) exist.

## Class: `FetchClient`
Retrieves email messages from the specified AWS S3 bucket, extracts attachments, and saves them.

## Class: `MailMessage`

### Properties
- **From**: Sender of the email.
- **To**: Recipients of the email.
- **Subject**: Subject of the email.
- **Body**: Email content.
- **Date**: Date the email was sent.
- **Bucket**: The S3 bucket where the email is stored.
- **MessageId**: Unique identifier for the email.
- **Attachments**: Associated file attachments.

## Class: `MailMessageAttachment`

### Properties
- **MailMessageId**: ID of the associated email.
- **Attachment**: The file attachment data.

## Usage Example
```csharp
await Mailbox.Watch("my-email-bucket");
await Mailbox.FetchAll();
```

## Conclusion
The `Olive.Aws.Ses.AutoFetch` simplifies AWS SES email retrieval by monitoring an S3 bucket, processing messages, extracting attachments, and storing them efficiently in a database. Proper configuration of AWS credentials and database connections is required for smooth operation.

