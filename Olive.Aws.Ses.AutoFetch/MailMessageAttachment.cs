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
        /// <summary>Stores the binary information for Attachment property.</summary>
        private Blob attachment;

        CachedReference<MailMessage> cachedMailMessage = new CachedReference<MailMessage>();

        /// <summary>Gets or sets the value of Attachment on this Mail message attachment instance.</summary>
        [Newtonsoft.Json.JsonIgnore]
        public Blob Attachment
        {
            get
            {
                if (attachment is null) attachment = Blob.Empty().Attach(this, "Attachment");
                return attachment;
            }

            set
            {
                if (!(attachment is null))
                {
                    // Detach the previous file, so it doesn't get updated or deleted with this Mail message attachment instance.
                    attachment.Detach();
                }

                if (value is null)
                {
                    value = Blob.Empty();
                }

                attachment = value.Attach(this, "Attachment");
            }
        }

        /// <summary>Gets or sets the ID of the associated MailMessage.</summary>
        public Guid? MailMessageId { get; set; }

        /// <summary>Gets or sets the value of MailMessage on this Mail message attachment instance.</summary>
        [System.ComponentModel.DisplayName("MailMessage")]
        public MailMessage MailMessage
        {
            get => cachedMailMessage.GetOrDefault(MailMessageId);
            set => MailMessageId = value?.ID;
        }

        /// <summary>Returns a textual representation of this Mail message attachment.</summary>
        public override string ToString() => $"Mail message attachment ({ID})";

        /// <summary>Returns a clone of this Mail message attachment.</summary>
        /// <returns>
        /// A new Mail message attachment object with the same ID of this instance and identical property values.<para/>
        ///  The difference is that this instance will be unlocked, and thus can be used for updating in database.<para/>
        /// </returns>
        public new MailMessageAttachment Clone()
        {
            var result = (MailMessageAttachment)base.Clone();

            result.Attachment = Attachment.Clone();
            return result;
        }

        /// <summary>
        /// Validates the data for the properties of this Mail message attachment and throws a ValidationException if an error is detected.<para/>
        /// </summary>
        protected override Task ValidateProperties()
        {
            var result = new List<string>();

            // Ensure the file uploaded for Attachment is safe:

            if (Attachment.HasValue() && Attachment.HasUnsafeExtension()) result.Add("The file uploaded for Attachment is unsafe because of the file extension: {0}".FormatWith(Attachment.FileExtension));

            if (result.Any())
                throw new ValidationException(result.ToLinesString());

            return Task.CompletedTask;
        }
    }
}