using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MimeKit;
using Olive.Email;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Olive.Aws.Ses
{
    public class AwsSesEmailDispatcher : IEmailDispatcher
    {
        public async Task Dispatch(MailMessage mail, IEmailMessage _)
        {
            var request = new SendRawEmailRequest { RawMessage = GetMessage(mail, _).ToRawMessage() };

            using (var client = new AmazonSimpleEmailServiceClient())
            {
                var response = await client.SendRawEmailAsync(request);

                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception("Failed to send an email: " + response.HttpStatusCode);
            }
        }

        MimeMessage GetMessage(MailMessage mail, IEmailMessage _ = null)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(mail.From?.DisplayName, mail.From.Address));
            mail.To.Do(t => message.To.Add(new MailboxAddress(t.DisplayName, t.Address)));
            mail.Bcc.Do(b => message.Bcc.Add(new MailboxAddress(b.DisplayName, b.Address)));
            mail.CC.Do(c => message.Cc.Add(new MailboxAddress(c.DisplayName, c.Address)));
            message.Subject = mail.Subject;
            message.Body = CreateMessageBody(mail, _);
            return message;
        }

        MimeEntity CreateMessageBody(MailMessage mail, IEmailMessage _ = null)
        {
            var body = new BodyBuilder()
            {
                HtmlBody = mail.Body
            };

            if (_ != null && _.VCalendarView.HasValue())
                body.HtmlBody += _.VCalendarView;

            foreach (var attc in mail.Attachments)
                body.Attachments.Add(attc.Name, attc.ContentStream);


            return body.ToMessageBody();
        }
    }
}