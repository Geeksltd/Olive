using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Email
{
    /// <summary>
    /// Provides email sending services.
    /// </summary>
    public static partial class EmailService
    {
        static AsyncLock AsyncLock = new AsyncLock();
        static Random Random = new Random();
        public static int MaximumRetries => Config.Get("Email:MaximumRetries", 4);

        /// <summary>
        /// Occurs when the smtp mail message for this email is about to be sent.
        /// </summary>
        public static readonly AsyncEvent<EmailSendingEventArgs> Sending = new AsyncEvent<EmailSendingEventArgs>();

        /// <summary>
        /// Occurs when the smtp mail message for this email is sent. Sender is the IEmailQueueItem instance that was sent.
        /// </summary>
        public static readonly AsyncEvent<EmailSendingEventArgs> Sent = new AsyncEvent<EmailSendingEventArgs>();

        /// <summary>
        /// Occurs when an exception happens when sending an email. Sender parameter will be the IEmailQueueItem instance that couldn't be sent.
        /// </summary>
        public static readonly AsyncEvent<EmailSendingEventArgs> SendError = new AsyncEvent<EmailSendingEventArgs>();

        internal static bool IsSendingPermitted(string to)
        {
            var permittedDomains = Config.Get("Email:Permitted:Domains",
                defaultValue: "geeks.ltd.uk|uat.co").ToLowerOrEmpty();
            if (permittedDomains == "*") return true;

            if (permittedDomains.Split('|').Trim().Any(d => to.TrimEnd(">").EndsWith("@" + d))) return true;

            var permittedAddresses = Config.Get("Email:Permitted:Addresses").ToLowerOrEmpty().Split('|').Trim();

            return permittedAddresses.Any() && new MailAddress(to).Address.IsAnyOf(permittedAddresses);
        }

        static IDatabase Database => Context.Current.Database();

        /// <summary>Tries to sends all emails.</summary>
        public static async Task SendAll(TimeSpan? delayPerSend = null)
        {
            using (await AsyncLock.Lock())
            {
                foreach (var mail in (await Database.GetList<IEmailMessage>()).OrderBy(e => e.SendableDate).ToArray())
                {
                    if (mail.Retries >= MaximumRetries) continue;

                    if (delayPerSend > TimeSpan.Zero)
                    {
                        var multiply = 1 + (Random.NextDouble() - 0.5) / 4; // from 0.8 to 1.2

                        try { await Task.Delay(delayPerSend.Value.Multiply(multiply)); }
                        catch (ThreadAbortException)
                        {
                            // Application terminated.
                            return;
                        }
                    }

                    try
                    {
                        await mail.Send();
                    }
                    catch (Exception ex)
                    {
                        Log.For(typeof(EmailService))
                            .Error(ex, "Could not send a queued email item " + mail.GetId());
                    }
                }
            }
        }

        /// <summary>
        /// Will try to send the specified email and returns true for successful sending.
        /// </summary>
        public static async Task<bool> Send(IEmailMessage mailItem)
        {
            if (mailItem == null) throw new ArgumentNullException(nameof(mailItem));
            if (mailItem.Retries >= MaximumRetries) return false;

            MailMessage mail = null;

            try
            {
                using (mail = await CreateMailMessage(mailItem))
                {
                    if (mail == null) return false;
                    await Sending.Raise(new EmailSendingEventArgs(mailItem, mail));

                    var dispatcher = Context.Current.GetOptionalService<IEmailDispatcher>() ??
                        new DefaultEmailDispatcher();
                    var result = await dispatcher.Dispatch(mailItem, mail);
                    await Sent.Raise(new EmailSendingEventArgs(mailItem, mail));
                    return result;
                }
            }
            catch (Exception ex)
            {
                await SendError.Raise(new EmailSendingEventArgs(mailItem, mail) { Error = ex });
                await mailItem.RecordRetry();
                Log.For(typeof(EmailService))
                    .Error(ex, $"Error in sending an email for this EmailQueueItem of '{mailItem.GetId()}'");
                return false;
            }
        }

        /// <summary>
        /// Gets the email items which have been sent (marked as soft deleted).
        /// </summary>
        public static async Task<IEnumerable<T>> GetSentEmails<T>() where T : IEmailMessage
        {
            using (new SoftDeleteAttribute.Context(bypassSoftdelete: false))
            {
                return (await Database.GetList<T>())
                    .Where(x => EntityManager.IsSoftDeleted((Entity)(IEntity)x));
            }
        }

        /// <summary>
        /// Creates an SMTP mail message for a specified mail item.
        /// </summary>
        static async Task<MailMessage> CreateMailMessage(IEmailMessage mailItem)
        {
            if (mailItem.SendableDate > LocalTime.Now) return null; // Not due yet

            var mail = new MailMessage { Subject = mailItem.Subject.Or("[NO SUBJECT]").Remove("\r", "\n") };

            mailItem.GetEffectiveToAddresses().Do(x => mail.To.Add(x));
            mailItem.GetEffectiveCcAddresses().Do(x => mail.CC.Add(x));
            mailItem.GetEffectiveBccAddresses().Do(x => mail.Bcc.Add(x));

            if (mail.To.None() && mail.CC.None() && mail.Bcc.None())
            {
                Debug.WriteLine($"Mail message {mailItem.GetId()} will not be sent as there is no effective recipient.");
                return null;
            }

            mail.AlternateViews.AddRange(mailItem.GetEffectiveBodyViews());

            mail.From = new MailAddress(mailItem.GetEffectiveFromAddress(), mailItem.GetEffectiveFromName());

            mail.ReplyToList.Add(new MailAddress(mailItem.GetEffectiveReplyToAddress(),
                mailItem.GetEffectiveReplyToName()));

            mail.Attachments.AddRange(await mailItem.GetAttachments());

            return mail;
        }

        /// <summary>
        /// Creates a VCalendar text with the specified parameters.
        /// </summary>
        /// <param name="meetingUniqueIdentifier">This uniquely identifies the meeting and is used for changes / cancellations. It is recommended to use the ID of the owner object.</param>
        public static string CreateVCalendarView(string meetingUniqueIdentifier, DateTime start, DateTime end, string subject, string description, string location)
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

            return r.ToString();
        }
    }
}