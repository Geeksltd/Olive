using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Olive.Entities;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Email
{
    public static partial class EmailExtensions
    {
        public static IServiceCollection AddEmail(this IServiceCollection @this)
        {
            return @this
                 .AddSingleton<IEmailAttachmentSerializer, EmailAttachmentSerializer>()
                 .AddSingleton<IEmailDispatcher, EmailDispatcher>()
                 .AddSingleton<IEmailSender, EmailSender>()
                 .AddSingleton<IMailMessageCreator, MailMessageCreator>();
        }

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
        /// Creates a VCalendar text with the specified parameters.
        /// </summary>
        /// <param name="meetingUniqueIdentifier">This uniquely identifies the meeting and is used for changes / cancellations. It is recommended to use the ID of the owner object.</param>
        public static string AddVCalendarView(this IEmailMessage @this, string meetingUniqueIdentifier, DateTime start, DateTime end, string subject, string description, string location)
        {
            var dateFormat = "yyyyMMddTHHmmssZ";

            Func<string, string> cleanUp = s => s.Or("").Remove("\r").Replace("\n", "\\n");

            var r = new StringBuilder();
            r.AppendLine(@"BEGIN:VCALENDAR");
            r.AppendLine(@"PRODID:-//Microsoft Corporation//Outlook 12.0 MIMEDIR//EN");
            r.AppendLine(@"VERSION:1.0");
            r.AppendLine(@"BEGIN:VEVENT");

            r.AddFormattedLine(@"DTSTART:{0}", start.ToString(dateFormat));
            r.AddFormattedLine(@"DTEND:{0}", end.ToString(dateFormat));
            r.AddFormattedLine(@"UID:{0}", meetingUniqueIdentifier);
            r.AddFormattedLine(@"SUMMARY:{0}", cleanUp(subject));
            r.AppendLine("LOCATION:" + cleanUp(location));
            r.AppendLine("DESCRIPTION:" + cleanUp(description));

            // bodyCalendar.AppendLine(@"PRIORITY:3");
            r.AppendLine(@"END:VEVENT");
            r.AppendLine(@"END:VCALENDAR");

            return @this.VCalendarView = r.ToString();
        }
    }
}