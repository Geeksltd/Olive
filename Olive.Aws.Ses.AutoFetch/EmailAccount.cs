using MimeKit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Olive.Aws.Ses.AutoFetch
{
    public class Attachment
    {
        public string FileName { get; set; }
        public string Base64 { get; set; }

        public Attachment() { }
        public Attachment(MimeEntity data)
        {
            FileName = data.ContentDisposition.FileName;

            using (var memory = new MemoryStream())
            {
                if (data is MimePart part) part.Content.DecodeTo(memory);
                else if (data is MessagePart pa) pa.Message.WriteTo(memory);
                else throw new NotSupportedException();
                Base64 = memory.ReadAllBytes().ToBase64String();
            }
        }
    }

    abstract class EmailAccount
    {
        public string S3Bucket { get; private set; }
        EmailAccount()
        {
        }

        internal EmailAccount(string s3Bucket) => S3Bucket = s3Bucket;

        protected abstract IMailMessage CreateMailMessageInstance();

        internal IMailMessage CreateMailMessage(MimeMessage message, string bucket)
        {
            var result = CreateMailMessageInstance();

            result.From = message.From.Select(f => f.ToString()).ToString(",");
            result.To = message.To.Select(f => f.ToString()).ToString(",");
            result.Cc = message.Cc?.ToString();
            result.Bcc = message.Bcc?.ToString();
            result.Body = message.HtmlBody.Or(message.TextBody);
            result.Date = message.Date.DateTime;
            result.Sender = message.Sender?.ToString();
            result.Subject = message.Subject;
            result.Bucket = bucket;
            result.Attachments = JsonConvert.SerializeObject(message.Attachments.OrEmpty().Where(x => x.IsAttachment).Select(x => new Attachment(x)).ToArray());

            return result;
        }
    }
}