1. Set up the bucket
https://aws.amazon.com/premiumsupport/knowledge-center/ses-receive-inbound-emails/

In Startup.cs, kick start the fetching engine.

using Olive.Aws.Ses.AutoFetch;

public override void Configure(IApplicationBuilder app)
{
    ....
    // Option 1: Use the built-in default type of AutoFetch.MailMessage.
   await Mailbox.Watch("s3 bucket name where SES is pointing to");

    // Option 2: Use your own type, which should implement AutoFetch.IMailMessage & AutoFetch.IMailMessageAttachment (recommendation).
    await Mailbox.Watch<Domain.MailMessage,Domain.MailMessageAttachment>("s3 bucket name where SES is pointing to");
}


NOTE
==================================================
If Option 1 is used, upon application startup, the following database table will
be automatically created for store records of AutoFetch.MailMessage class.

CREATE TABLE [MailMessage]
...

CREATE TABLE [MailMessageAttachments]
...

If Option 2 is used, add these entities

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
            Bool("Processed").Mandatory();

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



EXECUTION
==================================================
Create a background process to call the following:
    await Olive.Aws.Ses.AutoFetch.Mailbox.FetchAll();


Convert old attachments to physical file
==================================================
1. Add the following methods to the MailMessage logic class:

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

2. Create a background process to call the following:
    await Domain.MailMessage.ConvertOldAttachmentsToPhysicalFile();