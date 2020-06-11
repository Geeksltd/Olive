using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olive.Aws.Ses.AutoFetch
{
    abstract class EmailAccount
    {
        public string S3Bucket { get; private set; }
        EmailAccount()
        {

        }

        internal EmailAccount(string s3Bucket)
        {
            S3Bucket = s3Bucket;
        }

        protected abstract IMailMessage CreateMailMessageInstance();

        internal IMailMessage CreateMailMessage(MimeMessage message)
        {
            var result = CreateMailMessageInstance();

            result.From = message.From.Select(f => f.ToString()).ToString(",");
            result.To = message.To.Select(f => f.ToString()).ToString(",");
            result.Cc = message.Cc?.ToString();
            result.Bcc = message.Bcc?.ToString();
            result.Body = message.HtmlBody;
            result.Date = message.Date.DateTime;
            result.Sender = message.Sender?.ToString();
            result.Subject = message.Subject;


            return result;
        }
    }
}
