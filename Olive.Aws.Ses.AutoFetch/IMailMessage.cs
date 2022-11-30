using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;
using Newtonsoft.Json;
using Olive.Entities;
using Olive.Entities.Data;

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
    }

    public static class MailMessageExtensions
    {
        public static Attachment[] GetAttachments(this IMailMessage @this)
        {
            var resualt = new List<Attachment>();
            var attachments = Task.Factory.RunSync(() => Context.Current.Database().Of<IMailMessageAttachment>()
                 .Where(m => m.MailMessageId == @this.GetId().ToString().TryParseAs<Guid>()).GetList());
            foreach (var attachment in attachments)
            {
                var base64 = Task.Factory.RunSync(() => attachment.Attachment.GetFileDataAsync());
                resualt.Add(new Attachment
                {
                    FileName = attachment.Attachment.FileName,
                    Base64 = base64.ToBase64String()
                });
            }
            return resualt.ToArray();
        }
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
