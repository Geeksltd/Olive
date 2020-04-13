using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Olive.Email;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Olive.Aws.Ses
{
    public class AwsSesEmailDispatcher : IEmailDispatcher
    {
        public async Task Dispatch(MailMessage mail, IEmailMessage _)
        {
            var request = CreateEmailRequest(mail);

            using (var client = new AmazonSimpleEmailServiceClient())
            {
                var response = await client.SendEmailAsync(request);
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception("Failed to send an email: " + response.HttpStatusCode);
            }
        }

        SendEmailRequest CreateEmailRequest(MailMessage mail)
        {
            return new SendEmailRequest
            {
                Source = mail.From.Address,
                Destination = new Destination
                {
                    ToAddresses = mail.To.Select(t => t.Address).ToList()
                },
                Message = new Message
                {
                    Subject = new Content(mail.Subject),
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = "UTF-8",
                            Data = mail.Body
                        }
                    }
                }
            };
        }
    }
}
