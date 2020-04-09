using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olive.Aws.Ses.AutoFetch
{
    class EmailAccount
    {
        public string S3Bucket { get; private set; }
        EmailAccount()
        {

        }

        internal EmailAccount(string s3Bucket)
        {
            S3Bucket = s3Bucket;
        }

        protected virtual IMailMessage CreateMailMessageInstance() => new MailMessage();

        internal IMailMessage CreateMailMessage(MimeMessage message)
        {
            var result = CreateMailMessageInstance();

            result.From = message.From.Select(f => f.Name).ToString(",");
            result.To = message.To.Select(f => f.Name).ToString(",");
            result.HtmlBody = message.HtmlBody;
            result.Date = message.Date.DateTime;
            result.Sender = message.Sender?.Name;
            result.Subject = message.Subject;


            return result;
        }
    }
}
