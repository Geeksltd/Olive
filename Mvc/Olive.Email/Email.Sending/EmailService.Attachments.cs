using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Olive.Email
{
    partial class EmailService
    {
        /// <summary>
        /// Gets the Attachment objects to be attached to this email.
        /// </summary>
        public static Task<IEnumerable<Attachment>> GetAttachments(this IEmailMessage @this)
        {
            var result = @this.Attachments.OrEmpty().Split('|').Trim()
                 .Select(ParseAttachment)
                 .ExceptNull().ToList();

            return Task.FromResult((IEnumerable<Attachment>)result);
        }

        public static Attachment ParseAttachment(string attachmentInfo)
        {
            if (attachmentInfo.StartsWith("{"))
            {
                return GetAttachmentFromJSon(attachmentInfo);
            }
            else
            {
                if (attachmentInfo.StartsWith("\\\\") || Path.IsPathRooted(attachmentInfo))
                    return new Attachment(attachmentInfo); // absolute path
                else
                    return new Attachment(Path.Combine(AppDomain.CurrentDomain.WebsiteRoot().FullName, attachmentInfo));
            }
        }

        static Attachment GetAttachmentFromJSon(string attachmentInfo)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(attachmentInfo);

            if (data == null) return null;

            var contents = data.GetOrDefault("Contents") as string;

            if (contents.HasValue())
            {
                if (data.GetOrDefault("IsLinkedResource").ToStringOrEmpty().TryParseAs<bool>() == true) return null;
                // No attachment needed?

                var stream = new MemoryStream(Convert.FromBase64String(contents));
                var name = data["Name"] as string;
                var contentId = data["ContentId"] as string;

                return new Attachment(stream, name) { ContentId = contentId };
            }

            return null;
        }

        /// <summary>
        /// Gets the Linked Resource objects to be attached to this email.
        /// </summary>
		[EscapeGCop("It would cause error if you dispose the result.")]
        public static IEnumerable<LinkedResource> GetLinkedResources(this IEmailMessage mail)
        {
            if (mail.Attachments.HasValue())
            {
                foreach (var resource in mail.Attachments.Trim().Split('|').Where(x => x.StartsWith("{")))
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(resource);

                    if (data == null) continue;

                    var contents = data.GetOrDefault("Contents") as string;

                    if (contents.IsEmpty()) continue;

                    var isLinkedResource = data.GetOrDefault("IsLinkedResource").ToStringOrEmpty().TryParseAs<bool>() ?? false;

                    if (!isLinkedResource) continue;

                    var stream = new MemoryStream(Convert.FromBase64String(contents));
                    var name = data["Name"] as string;
                    var contentId = data["ContentId"] as string;

                    yield return new LinkedResource(stream)
                    {
                        ContentId = contentId,
                        ContentType = new System.Net.Mime.ContentType { Name = name }
                    };
                }
            }
        }
    }
}