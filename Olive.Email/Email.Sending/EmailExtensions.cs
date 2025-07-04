﻿using Microsoft.Extensions.DependencyInjection;
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
                 .AddTransient<IEmailAttachmentSerializer, EmailAttachmentSerializer>()
                 .AddTransient<IEmailDispatcher, EmailDispatcher>()
                 .AddTransient<IEmailRepository, EmailRepository>()
                 .AddTransient<IEmailOutbox, EmailOutbox>()
                 .AddTransient<IMailMessageCreator, MailMessageCreator>()
                 .AddTransient<IDevCommand, EmailTestDevCommand>();
        }

        /// <summary>
        /// Attaches a file to this email.
        /// </summary>
        public static async Task Attach(this IEmailMessage @this, Blob file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (file.IsEmpty()) return;
            @this.Attach(await file.GetFileDataAsync(), file.FileName);
        }

        /// <summary>
        /// Attaches a file to this email.
        /// </summary>
		/// <param name="this">The email queue item.</param>
        /// <param name="file">The path of the attachment file.
        /// This must be the physical path of a file inside the running application.</param>
        public static void Attach(this IEmailMessage @this, FileInfo file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var basePath = AppDomain.CurrentDomain.WebsiteRoot().FullName.ToLower();

            var path = file.FullName;

            if (path.StartsWith(basePath, caseSensitive: false)) // Relative:
                path = path.Substring(basePath.Length).TrimStart("\\");

            if (@this.Attachments.IsEmpty()) @this.Attachments = path;
            else @this.Attachments += "|" + path;
        }

        /// <summary>
        /// Attaches the specified byte array content to this email as an attachment.
        /// </summary>
        public static void Attach(this IEmailMessage @this, byte[] fileData, string name)
        {
            var data = new { Contents = fileData.ToBase64String(), Name = name };
            var json = JsonConvert.SerializeObject(data);

            if (@this.Attachments.IsEmpty()) @this.Attachments = json;
            else @this.Attachments += "|" + json;
        }

        /// <summary>
        /// Attaches the specified byte array content to this email, which will be used as a linked resource in the email body.
        /// </summary>
        public static void AttachLinkedResource(this IEmailMessage @this, byte[] fileData, string name, string contentId)
        {
            var data = new { Contents = fileData.ToBase64String(), Name = name, ContentId = contentId, IsLinkedResource = true };
            var json = JsonConvert.SerializeObject(data);

            if (@this.Attachments.IsEmpty()) @this.Attachments = json;
            else @this.Attachments += "|" + json;
        }

        /// <summary>
        /// Creates a VCalendar text with the specified parameters.
        /// </summary>
        /// <param name="meetingUniqueIdentifier">This uniquely identifies the meeting and is used for changes / cancellations. It is recommended to use the ID of the owner object.</param>         
        public static string AddVCalendarView(this IEmailMessage @this, string meetingUniqueIdentifier, DateTime start, DateTime end, string subject, string description, string location)
        {
            return @this.AddVCalendarView(new VCalendarEvent
            {
                UniqueIdentifier = meetingUniqueIdentifier,
                Start = start,
                End = end,
                Subject = subject,
                Description = description,
                Location = location
            });
        }

        public static string AddVCalendarView(this IEmailMessage @this, VCalendarEvent vCalEvent)
        {
            return @this.VCalendarView = vCalEvent.ToString();
        }

        /// <summary>
        /// Sends this error as a notification email to the address in web.config as Error.Notification.Receiver.
        /// </summary>
        public static IEmailMessage SendAsNotification(this Exception error)
        {
            return SendAsNotification(error, Config.Get("Error.Notification.Receiver"));
        }

        /// <summary>
        /// Sends this error as a notification email to the address in web.config as Error.Notification.Receiver.
        /// </summary>
        public static IEmailMessage SendAsNotification(this Exception error, string toNotify)
        {
            if (toNotify.IsEmpty()) return null;

            var email = InterfaceActivator.CreateInstance<IEmailMessage>();

            email.To = toNotify;
            email.Subject = "Error In Application";

            email.Body = $"URL: {Context.Current.Request()?.ToAbsoluteUri()}{Environment.NewLine}" +
                $"IP: {Context.Current.Http()?.Connection?.RemoteIpAddress}{Environment.NewLine}" +
                $"User: {Context.Current.User().GetId()}{Environment.NewLine}" +
                error.ToLogString(error.Message);

            Context.Current.Database().Save(email);

            return email;
        }
    }
}