using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Olive.Email;

/// <summary>
/// Creates <see cref="MailMessage"/> instances based on an <see cref="IEmailMessage"/>.
/// </summary>
public interface IMailMessageCreator
{
    Task<MailMessage> Create(IEmailMessage message);
}

/// <summary>
/// Default implementation of <see cref="IMailMessageCreator"/>.
/// </summary>
public sealed class MailMessageCreator : IMailMessageCreator
{
    private readonly EmailConfiguration _config;
    private readonly IEmailAttachmentSerializer _attachmentSerializer;

    public MailMessageCreator(IConfiguration config, IEmailAttachmentSerializer attachmentSerializer)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config.GetSection("Email").Get<EmailConfiguration>() ?? new EmailConfiguration();
        _attachmentSerializer = attachmentSerializer ?? throw new ArgumentNullException(nameof(attachmentSerializer));
    }

    public async Task<MailMessage> Create(IEmailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var result = new MailMessage
        {
            Subject = message.Subject.Or("[NO SUBJECT]").Remove("\r", "\n"),
            Body = message.Body,
            From = CreateFrom(message)
        };

        result.Headers.Add(Constants.EMAIL_MESSAGE_ID_HEADER_KEY, message.GetId().ToString());

        foreach (var recipient in GetEffectiveRecipients(message.To))
            result.To.Add(recipient);

        foreach (var recipient in GetEffectiveRecipients(message.Cc + _config.AutoAddCc.WithPrefix(",")))
            result.CC.Add(recipient);

        foreach (var recipient in GetEffectiveRecipients(message.Bcc + _config.AutoAddBcc.WithPrefix(",")))
            result.Bcc.Add(recipient);

        result.ReplyToList.Add(CreateReplyTo(message));

        result.AlternateViews.AddRange(GetBodyViews(message));
        result.Attachments.AddRange(await _attachmentSerializer.Extract(message));

        return result;
    }

    private MailAddress CreateFrom(IEmailMessage message) =>
        new(message.FromAddress.Or(_config.From?.Address),
            message.FromName.Or(_config.From?.Name));

    private MailAddress CreateReplyTo(IEmailMessage message)
    {
        var address = message.ReplyToAddress
            .Or(_config.ReplyTo?.Address)
            .Or(message.FromAddress)
            .Or(_config.From?.Address);

        var name = message.ReplyToName
            .Or(_config.ReplyTo?.Name)
            .Or(message.FromName)
            .Or(_config.From?.Name);

        return new MailAddress(address, name);
    }

    private IEnumerable<string> GetEffectiveRecipients(string? to) =>
        to.OrEmpty().Split(',').Trim().Where(CanSendTo);

    private bool CanSendTo(string recipientAddress)
    {
        var permittedDomains = _config.Permitted.Domains.ToLowerOrEmpty().Or("geeks.ltd.uk|uat.co");
        if (permittedDomains == "*") return true;

        if (permittedDomains.Split('|').Trim().Any(d => recipientAddress.TrimEnd(">").EndsWith("@" + d)))
            return true;

        var addresses = _config.Permitted.Addresses.ToLowerOrEmpty().Split('|').Trim().ToArray();

        return addresses.Any() && new MailAddress(recipientAddress).Address.IsAnyOf(addresses);
    }

    private IEnumerable<AlternateView> GetBodyViews(IEmailMessage message)
    {
        yield return AlternateView.CreateAlternateViewFromString(
            message.Body.RemoveHtmlTags(),
            new ContentType("text/plain; charset=UTF-8"));

        if (message.Html)
        {
            var htmlView = AlternateView.CreateAlternateViewFromString(
                message.Body,
                new ContentType("text/html; charset=UTF-8"));

            htmlView.LinkedResources.AddRange(_attachmentSerializer.GetLinkedResources(message));
            yield return htmlView;
        }

        if (message.VCalendarView.HasValue())
        {
            var calendarType = new ContentType("text/calendar");
            calendarType.Parameters.Add("method", "REQUEST");
            calendarType.Parameters.Add("name", "meeting.ics");

            var calendarView = AlternateView.CreateAlternateViewFromString(message.VCalendarView, calendarType);
            calendarView.TransferEncoding = TransferEncoding.SevenBit;

            yield return calendarView;
        }
    }
}

