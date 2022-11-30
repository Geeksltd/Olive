using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Aws.Ses.AutoFetch
{
    class EmailAccount<TMailMessage, TMailMessageAttachment> : EmailAccount 
        where TMailMessage : IMailMessage, new()
        where TMailMessageAttachment : IMailMessageAttachment, new()
    {
        internal EmailAccount(string s3Bucket) : base(s3Bucket)
        {
        }

        protected override IMailMessageAttachment CreateMailMessageAttachmentInstance() => new TMailMessageAttachment();

        protected override IMailMessage CreateMailMessageInstance() => new TMailMessage();
    }
}
