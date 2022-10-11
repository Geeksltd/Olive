using System;
using System.Collections.Generic;
using System.Linq;
using MimeKit;
using Newtonsoft.Json;
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
        string ReplyTo { get; set; }    
        /// <summary>
        /// Json array of Olive.Aws.Ses.AutoFetch.Attachment
        /// </summary>
        string Attachments { get; set; }
    }

    public static class MailMessageExtensions
    {
        public static Attachment[] GetAttachments(this IMailMessage @this)=> JsonConvert.DeserializeObject<Attachment[]>(@this.Attachments);
        public static IEnumerable<MailboxAddress> GetFroms(this IMailMessage @this) => ParseMailAddress(@this.From);
        public static IEnumerable<MailboxAddress> GetTos(this IMailMessage @this) => ParseMailAddress(@this.To);
        public static IEnumerable<MailboxAddress> GetCCs(this IMailMessage @this) => ParseMailAddress(@this.Cc);
        public static IEnumerable<MailboxAddress> GetReplyTos(this IMailMessage @this) => ParseMailAddress(@this.To);
        static IEnumerable<MailboxAddress> ParseMailAddress(string addressInfos)
        {
            InternetAddressList.TryParse(addressInfos, out var address);
            
            return address?.Mailboxes ?? Enumerable.Empty<MailboxAddress>();    
        }
    }
}
