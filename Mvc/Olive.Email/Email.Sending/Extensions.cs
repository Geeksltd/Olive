using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olive.Entities;

namespace Olive.Email
{
    public static partial class EmailExtensions
    {
        /// <summary>
        /// Attaches a file to this email.
        /// </summary>
        public static async Task Attach(this IEmailMessage mail, Blob file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (file.IsEmpty()) return;
            mail.Attach(await file.GetFileDataAsync(), file.FileName);
        }

        /// <summary>
        /// Attaches a file to this email.
        /// </summary>
		/// <param name="mail">The email queue item.</param>
        /// <param name="file">The path of the attachment file.
        /// This must be the physical path of a file inside the running application.</param>
        public static void Attach(this IEmailMessage mail, FileInfo file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var basePath = AppDomain.CurrentDomain.WebsiteRoot().FullName.ToLower();

            var path = file.FullName;

            if (path.StartsWith(basePath, caseSensitive: false)) // Relative:
                path = path.Substring(basePath.Length).TrimStart("\\");

            if (mail.Attachments.IsEmpty()) mail.Attachments = path;
            else mail.Attachments += "|" + path;
        }

        /// <summary>
        /// Attaches the specified byte array content to this email as an attachment.
        /// </summary>
        public static void Attach(this IEmailMessage mail, byte[] fileData, string name)
        {
            var data = new { Contents = fileData.ToBase64String(), Name = name };
            var json = JsonConvert.SerializeObject(data);

            if (mail.Attachments.IsEmpty()) mail.Attachments = json;
            else mail.Attachments += "|" + json;
        }

        /// <summary>
        /// Attaches the specified byte array content to this email, which will be used as a linked resource in the email body.
        /// </summary>
        public static void AttachLinkedResource(this IEmailMessage mail, byte[] fileData, string name, string contentId)
        {
            var data = new { Contents = fileData.ToBase64String(), Name = name, ContentId = contentId, IsLinkedResource = true };
            var json = JsonConvert.SerializeObject(data);

            if (mail.Attachments.IsEmpty()) mail.Attachments = json;
            else mail.Attachments += "|" + json;
        }

        /// <summary>
        /// Will send an email and returns true for successful sending.
        /// </summary>
        public static Task<bool> Send(this IEmailMessage mailItem) => EmailService.Send(mailItem);

        /// <summary>
        /// Records an unsuccessful attempt to send this email.
        /// </summary>
        public static async Task RecordRetry(this IEmailMessage emailItem)
        {
            if (emailItem.IsNew) throw new InvalidOperationException();

            var retries = emailItem.Retries + 1;

            if (!emailItem.IsNew)
                await Entity.Database.Update(emailItem, e => e.Retries = retries);

            // Also update this local instance:
            emailItem.Retries = retries;
        }

        public static string GetEffectiveFromName(this IEmailMessage mailItem)
          => mailItem.FromName.OrNullIfEmpty() ?? Config.GetOrThrow("Email:From:Name");

        public static string GetEffectiveFromAddress(this IEmailMessage mailItem)
            => mailItem.FromAddress.OrNullIfEmpty() ?? Config.GetOrThrow("Email:From:Address");

        public static string GetEffectiveReplyToName(this IEmailMessage mailItem)
            => mailItem.ReplyToName.OrNullIfEmpty() ?? Config.Get("Email:ReplyTo:Name") ?? mailItem.GetEffectiveFromName();

        public static string GetEffectiveReplyToAddress(this IEmailMessage mailItem)
            => mailItem.ReplyToAddress.OrNullIfEmpty() ??
            Config.Get("Email:ReplyTo:Address") ??
            mailItem.GetEffectiveFromAddress();

        public static string[] GetEffectiveToAddresses(this IEmailMessage mailItem)
            => mailItem.To.OrEmpty().Split(',').Trim().Where(a => EmailService.IsSendingPermitted(a)).ToArray();

        public static string[] GetEffectiveCcAddresses(this IEmailMessage mailItem)
        {
            var cc = Config.Get("Email:AutoAddCc").WithSuffix(",") + mailItem.Cc;
            return cc.OrEmpty().Split(',').Trim().Where(a => EmailService.IsSendingPermitted(a)).ToArray();
        }

        public static string[] GetEffectiveBccAddresses(this IEmailMessage mailItem)
        {
            var bcc = Config.Get("Email:AutoAddBcc").WithSuffix(",") + mailItem.Bcc;
            return bcc.OrEmpty().Split(',').Trim().Where(a => EmailService.IsSendingPermitted(a)).ToArray();
        }

        public static IEnumerable<AlternateView> GetEffectiveBodyViews(this IEmailMessage mailItem)
        {
            yield return AlternateView.CreateAlternateViewFromString(mailItem.Body.RemoveHtmlTags(),
            new ContentType("text/plain; charset=UTF-8"));

            if (mailItem.Html)
            {
                var htmlView = AlternateView.CreateAlternateViewFromString(
                    mailItem.Body, new ContentType("text/html; charset=UTF-8"));
                htmlView.LinkedResources.AddRange(mailItem.GetLinkedResources());
                yield return htmlView;
            }

            if (mailItem.VCalendarView.HasValue())
            {
                var calendarType = new ContentType("text/calendar");
                calendarType.Parameters.Add("method", "REQUEST");
                calendarType.Parameters.Add("name", "meeting.ics");

                var calendarView = AlternateView.CreateAlternateViewFromString(mailItem.VCalendarView, calendarType);
                calendarView.TransferEncoding = TransferEncoding.SevenBit;

                yield return calendarView;
            }
        }
    }
}