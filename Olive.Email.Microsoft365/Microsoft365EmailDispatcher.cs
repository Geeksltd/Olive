using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using Olive.Email;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Attachment = Microsoft.Graph.Models.Attachment;
using FileAttachment = Microsoft.Graph.Models.FileAttachment;

namespace Olive.Email.Microsoft365
{
    public class Microsoft365EmailDispatcher : IEmailDispatcher
    {
        public async Task Dispatch(MailMessage mail, IEmailMessage message)
        {
            var tenantId = Config.GetOrThrow("Email:Microsoft365:TenantId");
            var clientId = Config.GetOrThrow("Email:Microsoft365:ClientId");
            var clientSecret = Config.GetOrThrow("Email:Microsoft365:ClientSecret");
            var senderAddress = Config.GetOrThrow("Email:Microsoft365:SenderAddress");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            using var graphClient = new GraphServiceClient(credential);

            var requestBody = new SendMailPostRequestBody
            {
                Message = new Message
                {
                    Subject = mail.Subject,
                    Body = new ItemBody
                    {
                        ContentType = message.Html ? BodyType.Html : BodyType.Text,
                        Content = mail.Body
                    },
                    ToRecipients = ToRecipients(mail.To),
                    CcRecipients = ToRecipients(mail.CC),
                    BccRecipients = ToRecipients(mail.Bcc),
                    ReplyTo = ToRecipients(mail.ReplyToList),
                    Attachments = CreateAttachments(mail, message)
                }
            };

            await graphClient.Users[senderAddress].SendMail.PostAsync(requestBody);
        }

        static List<Recipient> ToRecipients(MailAddressCollection addresses)
            => addresses.Select(x => new Recipient
            {
                EmailAddress = new EmailAddress { Address = x.Address }
            }).ToList();

        static List<Attachment> CreateAttachments(MailMessage mail, IEmailMessage message)
        {
            var result = new List<Attachment>();

            foreach (var attachment in mail.Attachments)
            {
                result.Add(new FileAttachment
                {
                    OdataType = "#microsoft.graph.fileAttachment",
                    Name = attachment.Name,
                    ContentBytes = ReadBytes(attachment.ContentStream),
                    ContentType = attachment.ContentType.MediaType
                });
            }

            if (message.VCalendarView.HasValue())
            {
                result.Add(new FileAttachment
                {
                    OdataType = "#microsoft.graph.fileAttachment",
                    Name = "meeting.ics",
                    ContentBytes = Encoding.UTF8.GetBytes(message.VCalendarView),
                    ContentType = "text/calendar"
                });
            }

            return result.None() ? null : result;
        }

        static byte[] ReadBytes(Stream stream)
        {
            if (stream is MemoryStream memoryStream)
                return memoryStream.ToArray();

            using var copy = new MemoryStream();
            stream.CopyTo(copy);
            return copy.ToArray();
        }
    }
}
