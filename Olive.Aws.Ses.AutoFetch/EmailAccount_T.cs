using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Aws.Ses.AutoFetch
{
    class EmailAccount<TMailMessage> : EmailAccount where TMailMessage : IMailMessage, new()
    {
        internal EmailAccount(string s3Bucket) : base(s3Bucket)
        {
        }

        protected override IMailMessage CreateMailMessageInstance() => new TMailMessage();
    }
}
