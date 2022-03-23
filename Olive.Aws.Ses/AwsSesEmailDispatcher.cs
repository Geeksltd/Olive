using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MimeKit;
using Olive.Email;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Olive.Aws.Ses
{
    public class AwsSesEmailDispatcher : IEmailDispatcher
    {
        public async Task Dispatch(MailMessage mail, IEmailMessage iEmailMessage)
        {
            var request = new SendRawEmailRequest { RawMessage = GetMessage(mail, iEmailMessage).ToRawMessage() };

            using (var client = new AmazonSimpleEmailServiceClient())
            {
                var response = await client.SendRawEmailAsync(request);

                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception("Failed to send an email: " + response.HttpStatusCode);
            }
        }

        MimeMessage GetMessage(MailMessage mail, IEmailMessage iEmailMessage = null)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(mail.From?.DisplayName, mail.From.Address));
            mail.To.Do(t => message.To.Add(new MailboxAddress(t.DisplayName, t.Address)));
            mail.Bcc.Do(b => message.Bcc.Add(new MailboxAddress(b.DisplayName, b.Address)));
            mail.CC.Do(c => message.Cc.Add(new MailboxAddress(c.DisplayName, c.Address)));
            message.Subject = mail.Subject;
            message.Body = CreateMessageBody(mail, iEmailMessage);
            return message;
        }

        MimeEntity CreateMessageBody(MailMessage mail, IEmailMessage iEmailMessage = null)
        {
            var body = new BodyBuilder()
            {
                HtmlBody = mail.Body
            };

            if (iEmailMessage != null && iEmailMessage.VCalendarView.HasValue())
                body.Attachments.Add(
                    "meeting.ics",
                    System.Text.Encoding.Unicode.GetBytes(iEmailMessage.VCalendarView),
                    new MimeKit.ContentType("text", "calendar"));

            foreach (var attc in mail.Attachments)
                body.Attachments.Add(attc.Name, attc.ContentStream);

            return body.ToMessageBody();
        }
    }
}
