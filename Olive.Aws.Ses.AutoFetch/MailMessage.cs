using Olive.Aws.Ses.AutoFetch;
using Olive.Entities;
using Olive;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Aws.Ses.AutoFetch
{
    public class MailMessage : Olive.Entities.GuidEntity, IMailMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Bcc { get; set; }
        public string Cc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Sender { get; set; }
        public DateTime Date { get; set; }

        protected override async Task ValidateProperties()
        {
            await base.ValidateProperties();

            if (From.IsEmpty())
                throw new ValidationException($"Please provide a value for {nameof(From)}");

        }
    }
}
