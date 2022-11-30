using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Aws.Ses.AutoFetch
{
    public class MailMessageAttachment : GuidEntity, IMailMessageAttachment
    {
        public Guid? MailMessageId { get; set; }
        public Blob Attachment { get; set; }        

        protected override async Task ValidateProperties()
        {
            await base.ValidateProperties();

            var result = new List<string>();

            if (Attachment.IsEmpty())
            {
                result.Add("It is necessary to upload Attachment.");
            }

            // Ensure the file uploaded for Attachment is safe:

            if (Attachment.HasUnsafeExtension()) result.Add("The file uploaded for Attachment is unsafe because of the file extension: {0}".FormatWith(Attachment.FileExtension));

            if (MailMessageId == null)
                result.Add("Please provide a value for MailMessage.");

            if (result.Any())
                throw new ValidationException(result.ToLinesString());
        }
    }
}