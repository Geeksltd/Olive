1. Set up the bucket
https://aws.amazon.com/premiumsupport/knowledge-center/ses-receive-inbound-emails/

In Startup.cs, kick start the fetching engine.

using Olive.Aws.Ses.AutoFetch;

public override void Configure(IApplicationBuilder app)
{
    ....
    // Option 1: Use the built-in default type of AutoFetch.MailMessage.
   await Mailbox.Watch("s3 bucket name where SES is pointing to");

    // Option 2: Use your own type, which should implement AutoFetch.IMailMessage.
    await Mailbox.Watch<MyOwnMessageType>("s3 bucket name where SES is pointing to");
}


NOTE
==================================================
If Option 1 is used, upon application startup, the following database table will
be automatically created for store records of AutoFetch.MailMessage class.

CREATE TABLE [MailMessage]
...


EXECUTION
==================================================
Create a background process to call the following:
    await new Mailbox().FetchAll();