using System;
using Olive.Entities;

namespace Olive.Aws.Ses.AutoFetch
{
    public interface IMailMessage : IEntity
    {
        string From { get; set; }
        string To { get; set; }
        string Bcc { get; set; }
        string Cc { get; set; }
        string Subject { get; set; }
        string Body { get; set; }
        string Sender { get; set; }
        DateTime Date { get; set; }
        DateTime DateDownloaded { get; set; }
        string Bucket { get; set; }
        string MessageId { get; set; }

        /// <summary>
        /// Json array of Olive.Aws.Ses.AutoFetch.Attachment
        /// </summary>
        string Attachments { get; set; }
    }
}
