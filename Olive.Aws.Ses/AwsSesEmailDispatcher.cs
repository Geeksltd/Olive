using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Olive.Email;

namespace Olive.Aws.Ses
{
    public class AwsSesEmailDispatcher : IEmailDispatcher
    {
        // RuntimeIdentity.Credentials
        public async Task<bool> Dispatch(IEmailMessage email, MailMessage mail)
        {
            var request = CreateEmailRequest(email, mail);

            using (var client = new AmazonSimpleEmailServiceClient(RuntimeIdentity.Credentials, RuntimeIdentity.Region))
            {
                var response = await client.SendEmailAsync(request);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
        }

        private SendEmailRequest CreateEmailRequest(IEmailMessage email, MailMessage mail)
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
