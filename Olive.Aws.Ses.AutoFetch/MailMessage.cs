using System;
using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Aws.Ses.AutoFetch
{
    public class MailMessage : GuidEntity, IMailMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Bcc { get; set; }
        public string Cc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Sender { get; set; }
        public string Bucket { get; set; }
        public DateTime Date { get; set; }

        /// <summary>
        /// Json array of Olive.Aws.Ses.AutoFetch.Attachment
        /// </summary>
        public string Attachments { get; set; }

        protected override async Task ValidateProperties()
        {
            await base.ValidateProperties();

            if (From.IsEmpty())
                throw new ValidationException($"Please provide a value for {nameof(From)}");
        }
    }
}