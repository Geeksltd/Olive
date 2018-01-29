using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olive.Entities;

namespace Olive.Email
{
    public static class EmailExtensions
    {
        /// <summary>
        /// Gets the mandatory placeholder tokens for this template.
        /// </summary>
        public static IEnumerable<string> GetPlaceholderTokens(this IEmailTemplate template) =>
            template.MandatoryPlaceholders.Or("").Split(',').Trim().Select(t => $"[#{t.ToUpper()}#]");

        /// <summary>
        /// Ensures the mandatory placeholders are all specified in this template.
        /// </summary>
        public static void EnsurePlaceholders(this IEmailTemplate template)
        {
            // Make sure that all place holders appear in the email body or subject.
            var missingElements = template.GetPlaceholderTokens().Except(t => (template.Subject + template.Body).Contains(t));
            if (missingElements.Any())
                throw new ValidationException("Email template subject or body must have all place-holders for {0}. The missing ones are: {1}", template.Key, missingElements.ToString(", "));
        }

        /// <summary>
        /// Merges the subjcet of this email template with the specified data.
        /// </summary>
		/// <param name="template">The email template</param>
        /// <param name="mergeData">An anonymouse object. All property names should correspond to the placeholder names.
        /// For example: new {FirstName = GetFirstName() , LastName = "john"}</param>
        public static string MergeSubject(this IEmailTemplate template, object mergeData) => Merge(template.Subject, mergeData);

        /// <summary>
        /// Merges the body of this email template with the specified data.
        /// </summary>
        /// <param name="template">The email template</param>
        /// <param name="mergeData">An anonymouse object. All property names should correspond to the placeholder names.
        /// For example: new {FirstName = GetFirstName() , LastName = "john"}</param>
        public static string MergeBody(this IEmailTemplate template, object mergeData) => Merge(template.Body, mergeData);

        /// <summary>
        /// Merges the specified template with the provided.
        /// </summary>
        static string Merge(string template, object mergeData)
        {
            var result = template;

            foreach (var p in mergeData.GetType().GetProperties())
            {
                var key = $"[#{p.Name.ToUpper()}#]";
                var value = $"{p.GetValue(mergeData)}";

                result = result.Replace(key, value);
            }

            return result;
        }

        /// <summary>
        /// Attaches a file to this email.
        /// </summary>
        public static void Attach(this IEmailQueueItem mail, Blob file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (file.IsEmpty()) return;

            if (file.LocalPath.IsEmpty())
                throw new ArgumentException("In-memory blob instances cannot be used for email attachment. It should be saved on disk first.");

            var reference = file.GetOwnerPropertyReference();
            if (reference.HasValue())
            {
                var json = JsonConvert.SerializeObject(new { PropertyReference = reference });

                if (mail.Attachments.IsEmpty()) mail.Attachments = json;
                else mail.Attachments += "|" + json;
            }
            else
            {
                Attach(mail, file.LocalPath);
            }
        }

        /// <summary>
        /// Attaches a file to this email.
        /// </summary>
		/// <param name="mail">The email queue item.</param>
        /// <param name="filePath">The path of the attachment file.
        /// This must be the physical path of a file inside the running application.</param>
        public static void Attach(this IEmailQueueItem mail, string filePath)
        {
            if (filePath.IsEmpty()) throw new ArgumentNullException(nameof(filePath));

            var basePath = AppDomain.CurrentDomain.WebsiteRoot().FullName.ToLower();

            if (filePath.ToLower().StartsWith(basePath)) // Relative:
                filePath = filePath.Substring(basePath.Length).TrimStart("\\");

            if (mail.Attachments.IsEmpty()) mail.Attachments = filePath;
            else mail.Attachments += "|" + filePath;
        }

        /// <summary>
        /// Attaches the specified byte array content to this email as an attachment.
        /// </summary>
        public static void Attach(this IEmailQueueItem mail, byte[] fileData, string name, string contentId, bool isLinkedResource = false)
        {
            var data = new { Contents = fileData.ToBase64String(), Name = name, ContentId = contentId, IsLinkedResource = isLinkedResource };
            var json = JsonConvert.SerializeObject(data);

            if (mail.Attachments.IsEmpty()) mail.Attachments = json;
            else mail.Attachments += "|" + json;
        }

        /// <summary>
        /// Will send an email and returns true for successful sending.
        /// </summary>
        public static Task<bool> Send(this IEmailQueueItem mailItem) => EmailService.Send(mailItem);

        /// <summary>
        /// Records an unsuccessful attempt to send this email.
        /// </summary>
        public static async Task RecordRetry(this IEmailQueueItem emailItem)
        {
            if (emailItem.IsNew) throw new InvalidOperationException();

            var retries = emailItem.Retries + 1;

            if (!emailItem.IsNew)
                await Entity.Database.Update(emailItem, e => e.Retries = retries);

            // Also update this local instance:
            emailItem.Retries = retries;
        }
    }
}