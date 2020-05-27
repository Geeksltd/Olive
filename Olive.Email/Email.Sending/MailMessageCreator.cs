using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Olive.Email
{
    public interface IMailMessageCreator
    {
        Task<MailMessage> Create(IEmailMessage message);
    }

    class MailMessageCreator : IMailMessageCreator
    {
        EmailConfiguration Config;

        public MailMessageCreator(IConfiguration config, IEmailAttachmentSerializer attachmentSerializer)
        {
            Config = config.GetSection("Email").Get<EmailConfiguration>();
            AttachmentSerializer = attachmentSerializer;
        }

        public IEmailAttachmentSerializer AttachmentSerializer { get; }

        public async Task<MailMessage> Create(IEmailMessage message)
        {
            var result = new MailMessage
            {
                Subject = message.Subject.Or("[NO SUBJECT]").Remove("\r", "\n"),
                Body = message.Body,
                From = CreateFrom(message)
            };

            result.Headers
                .Add(Constants.EMAIL_MESSAGE_ID_HEADER_KEY, message.GetId().ToString());

            GetEffectiveRecipients(message.To).Do(x => result.To.Add(x));
            GetEffectiveRecipients(message.Cc + Config.AutoAddCc.WithPrefix(",")).Do(x => result.CC.Add(x));
            GetEffectiveRecipients(message.Bcc + Config.AutoAddBcc.WithPrefix(",")).Do(x => result.Bcc.Add(x));

            result.ReplyToList.Add(CreateReplyTo(message));

            result.AlternateViews.AddRange(GetBodyViews(message));
            result.Attachments.AddRange(await AttachmentSerializer.Extract(message));

            return result;
        }

        MailAddress CreateFrom(IEmailMessage message)
        {
            var address = message.FromAddress.Or(Config.From?.Address);
            var name = message.FromName.Or(Config.From?.Name);
            return new MailAddress(address, name);
        }

        MailAddress CreateReplyTo(IEmailMessage message)
        {
            var address = message.ReplyToAddress
                .Or(Config.ReplyTo?.Address)
                .Or(message.FromAddress)
                .Or(Config.From?.Address);

            var name = message.ReplyToName
                .Or(Config.ReplyTo?.Name)
                .Or(message.FromName)
                .Or(Config.From?.Name);

            return new MailAddress(address, name);
        }

        string[] GetEffectiveRecipients(string to)
        {
            return to.OrEmpty().Split(',').Trim().Where(x => CanSendTo(x)).ToArray();
        }

        bool CanSendTo(string recipientAddress)
        {
            var permittedDomains = Config.Permitted.Domains.ToLowerOrEmpty().Or("geeks.ltd.uk|uat.co");
            if (permittedDomains == "*") return true;

            if (permittedDomains.Split('|').Trim().Any(d => recipientAddress.TrimEnd(">").EndsWith("@" + d)))
                return true;

            var addresses = Config.Permitted.Addresses.ToLowerOrEmpty().Split('|').Trim().ToArray();

            return addresses.Any() && new MailAddress(recipientAddress).Address.IsAnyOf(addresses);
        }

        IEnumerable<AlternateView> GetBodyViews(IEmailMessage message)
        {
            yield return AlternateView.CreateAlternateViewFromString(
                message.Body.RemoveHtmlTags(),
            new ContentType("text/plain; charset=UTF-8"));

            if (message.Html)
            {
                var htmlView = AlternateView.CreateAlternateViewFromString(
                    message.Body, new ContentType("text/html; charset=UTF-8"));
                htmlView.LinkedResources.AddRange(AttachmentSerializer.GetLinkedResources(message));
                yield return htmlView;
            }

            if (message.VCalendarView.HasValue())
            {
                var calendarType = new ContentType("text/calendar");
                calendarType.Parameters.Add("method", "REQUEST");
                calendarType.Parameters.Add("name", "meeting.ics");

                var calendarView = AlternateView
                    .CreateAlternateViewFromString(message.VCalendarView, calendarType);
                calendarView.TransferEncoding = TransferEncoding.SevenBit;

                yield return calendarView;
            }
        }
    }
}