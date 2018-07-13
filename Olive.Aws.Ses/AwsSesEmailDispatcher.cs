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
            var request = CreateEmailRequest(mail);

            using (var client = new AmazonSimpleEmailServiceClient(RuntimeIdentity.Region))
            {
                var response = await client.SendEmailAsync(request);
                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
        }

        private SendEmailRequest CreateEmailRequest(MailMessage email)
        {
            return new SendEmailRequest
            {
                Source = email.From.Address,
                Destination = new Destination
                {
                    ToAddresses = email.To.Select(t => t.Address).ToList()
                },
                Message = new Message
                {
                    Subject = new Content(email.Subject),
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = "UTF-8",
                            Data = email.Body
                        }
                    }
                }
            };
        }
    }
}
