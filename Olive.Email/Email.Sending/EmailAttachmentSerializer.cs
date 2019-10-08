using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Olive.Email
{
    public interface IEmailAttachmentSerializer
    {
        Task<IEnumerable<Attachment>> Extract(IEmailMessage message);

        Attachment Parse(string attachmentInfo);

        /// <summary>
        /// Gets the Linked Resource objects to be attached to this email.
        /// </summary>
        IEnumerable<LinkedResource> GetLinkedResources(IEmailMessage message);
    }

    public class EmailAttachmentSerializer : IEmailAttachmentSerializer
    {
        /// <summary>
        /// Gets the Attachment objects to be attached to this email.
        /// </summary>
        public Task<IEnumerable<Attachment>> Extract(IEmailMessage message)
        {
            var result = message.Attachments.OrEmpty().Split('|').Trim()
                 .Select(Parse)
                 .ExceptNull().ToList();

            return Task.FromResult((IEnumerable<Attachment>)result);
        }

        public Attachment Parse(string attachmentInfo)
        {
            if (attachmentInfo.StartsWith("{"))
            {
                return ParseJson(attachmentInfo);
            }
            else if (attachmentInfo.StartsWith("\\\\") || Path.IsPathRooted(attachmentInfo))
                return new Attachment(attachmentInfo); // absolute path
            else
                return new Attachment(AppDomain.CurrentDomain.WebsiteRoot().PathCombine(attachmentInfo));
        }

        Attachment ParseJson(string attachmentInfo)
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
                var contentId = data.GetOrDefault("ContentId") as string;

                return new Attachment(stream, name) { ContentId = contentId };
            }

            return null;
        }

        [EscapeGCop("It would cause error if you dispose the result.")]
        public IEnumerable<LinkedResource> GetLinkedResources(IEmailMessage message)
        {
            foreach (var resource in message.Attachments.TrimOrEmpty().Split('|').Where(x => x.StartsWith("{")))
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