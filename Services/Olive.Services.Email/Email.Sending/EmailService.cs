namespace Olive.Services.Email
{
    /// <summary>
    /// Provides email sending services.
    /// </summary>
    public static partial class EmailService
    {
        const string ALL_CATEGORIES = "*";
        static Type concreteEmailQueueItemType;
        static AsyncLock AsyncLock = new AsyncLock();
        static Random Random = new Random();

        public static int MaximumRetries => Config.Get("Email:Maximum.Retries", 4);

        /// <summary>
        /// Specifies a factory to instantiate EmailQueueItem objects.
        /// </summary>
        public static Func<IEmailQueueItem> EmailQueueItemFactory = CreateEmailQueueItem;

        /// <summary>
        /// Provides a message which can dispatch an email message.
        /// Returns whether the message was sent successfully.
        /// </summary>
        public static Func<IEmailQueueItem, MailMessage, Task<bool>> EmailDispatcher = SendViaSmtp;

        #region Events

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

        #endregion

        #region Factory

        static IEmailQueueItem CreateEmailQueueItem()
        {
            if (concreteEmailQueueItemType != null)
                return Activator.CreateInstance(concreteEmailQueueItemType) as IEmailQueueItem;

            var possible = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.References(typeof(IEmailQueueItem).Assembly))
                .SelectMany(a => { try { return a.GetExportedTypes(); } catch { return new Type[0]; /* No logging needed */ } })
                .Where(t => t.IsClass && !t.IsAbstract && t.Implements<IEmailQueueItem>()).ToList();

            if (possible.Count == 0)
                throw new Exception("No type in the currently loaded assemblies implements IEmailQueueItem.");

            if (possible.Count > 1)
                throw new Exception("More than one type in the currently loaded assemblies implement IEmailQueueItem:" + possible.Select(x => x.FullName).ToString(" and "));

            concreteEmailQueueItemType = possible.Single();
            return CreateEmailQueueItem();
        }

        #endregion

        static bool IsSendingPermitted(string to)
        {
            var permittedDomains = Config.Get("Email:Permitted.Domains").Or("geeks.ltd.uk|uat.co").ToLowerOrEmpty();
            if (permittedDomains == "*") return true;

            if (permittedDomains.Split('|').Trim().Any(d => to.TrimEnd(">").EndsWith("@" + d))) return true;

            var permittedAddresses = Config.Get("Email:Permitted.Addresses").ToLowerOrEmpty().Split('|').Trim();

            return permittedAddresses.Any() && new MailAddress(to).Address.IsAnyOf(permittedAddresses);
        }

        /// <summary>
        /// Tries to sends all emails.
        /// </summary>
        public static Task SendAll() => SendAll(ALL_CATEGORIES, TimeSpan.Zero);

        /// <summary>
        /// Tries to sends all emails.
        /// </summary>
        /// <param name="category">The category of the emails to send. Use "*" to indicate "all emails".</param>
        public static Task SendAll(string category) => SendAll(category, TimeSpan.Zero);

        /// <summary>
        /// Tries to sends all emails.
        /// </summary>
        /// <param name="delay">The time to wait in between sending each outstanding email.</param>
        public static Task SendAll(TimeSpan delay) => SendAll(ALL_CATEGORIES, delay);

        /// <summary>
        /// Tries to sends all emails.
        /// </summary>
        /// <param name="category">The category of the emails to send. Use "*" to indicate "all emails".</param>
        public static async Task SendAll(string category, TimeSpan delay)
        {
            using (await AsyncLock.Lock())
            {
                foreach (var mail in (await Entity.Database.GetList<IEmailQueueItem>()).OrderBy(e => e.Date).ToArray())
                {
                    if (mail.Retries >= MaximumRetries) continue;

                    if (category != ALL_CATEGORIES)
                    {
                        if (category.IsEmpty() && mail.Category.HasValue()) continue;
                        if (category != mail.Category) continue;
                    }

                    if (delay > TimeSpan.Zero)
                    {
                        var multiply = 1 + (Random.NextDouble() - 0.5) / 4; // from 0.8 to 1.2

                        try
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(delay.TotalMilliseconds * multiply));
                        }
                        catch (ThreadAbortException)
                        {
                            // Application terminated.
                            return;
                        }
                    }

                    try
                    {
                        if (await mail.Send() && !mail.IsNew)
                            await Entity.Database.Delete(mail);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Could not send a queued email item " + mail.GetId(), ex);
                    }
                }
            }
        }

        /// <summary>
        /// Will try to send the specified email and returns true for successful sending.
        /// </summary>
        public static async Task<bool> Send(IEmailQueueItem mailItem)
        {
            if (mailItem == null) throw new ArgumentNullException(nameof(mailItem));

            if (mailItem.Retries >= MaximumRetries) return false;

            MailMessage mail = null;

            try
            {
                using (mail = await CreateMailMessage(mailItem))
                {
                    if (mail == null) return false;
                    return await EmailDispatcher(mailItem, mail);
                }
            }
            catch (Exception ex)
            {
                await SendError.Raise(new EmailSendingEventArgs(mailItem, mail) { Error = ex });
                await mailItem.RecordRetry();
                Log.Error($"Error in sending an email for this EmailQueueItem of '{mailItem.GetId()}'", ex);
                return false;
            }
        }

        static async Task<bool> SendViaSmtp(IEmailQueueItem mailItem, MailMessage mail)
        {
            // Developer note: Web.config setting for SSL is designed to take priority over the specific setting of the email.
            // If in your application you want the email item's setting to take priority, do this:
            //      1. Remove the 'Email->Enable.Ssl' setting from appsettings.json totally.
            //      2. If you need a default value, use  your application's Global Settings object and use that value everywhere you create an EmailQueueItem.
            using (var smtpClient = new SmtpClient { EnableSsl = Config.Get("Email:Enable.Ssl", mailItem.EnableSsl) })
            {
                smtpClient.Configure();

                if (mailItem.SmtpHost.HasValue())
                    smtpClient.Host = mailItem.SmtpHost;

                if (mailItem.SmtpPort.HasValue)
                    smtpClient.Port = mailItem.SmtpPort.Value;

                if (mailItem.Username.HasValue())
                    smtpClient.Credentials = new NetworkCredential(mailItem.Username, mailItem.Password.Or((smtpClient.Credentials as NetworkCredential).Get(c => c.Password)));

                if (Config.IsDefined("Email:Random.Usernames"))
                {
                    var userName = Config.Get("Email:Random.Usernames").Split(',').Trim().PickRandom();
                    smtpClient.Credentials = new NetworkCredential(userName, Config.Get("Email:Password"));
                }

                await Sending.Raise(new EmailSendingEventArgs(mailItem, mail));

                await smtpClient.SendMailAsync(mail);

                await Sent.Raise(new EmailSendingEventArgs(mailItem, mail));
            }

            return true;
        }

        /// <summary>
        /// Gets the email items which have been sent (marked as soft deleted).
        /// </summary>
        public static async Task<IEnumerable<T>> GetSentEmails<T>() where T : IEmailQueueItem
        {
            using (new SoftDeleteAttribute.Context(bypassSoftdelete: false))
            {
                return (await Entity.Database.GetList<T>())
                    .Where(x => EntityManager.IsSoftDeleted((Entity)(IEntity)x));
            }
        }

        /// <summary>
        /// Creates an SMTP mail message for a specified mail item.
        /// </summary>
        static async Task<MailMessage> CreateMailMessage(IEmailQueueItem mailItem)
        {
            // Make sure it's due:
            if (mailItem.Date > LocalTime.Now) return null;

            var mail = new MailMessage { Subject = mailItem.Subject.Or("[NO SUBJECT]").Remove("\r", "\n") };

            #region Set Body

            if (mailItem.Html)
            {
                var htmlView = AlternateView.CreateAlternateViewFromString(mailItem.Body, new ContentType("text/html; charset=UTF-8"));

                // Add Linked Resources
                htmlView.LinkedResources.AddRange(mailItem.GetLinkedResources());

                mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(mailItem.Body.RemoveHtmlTags(), new ContentType("text/plain; charset=UTF-8")));
                mail.AlternateViews.Add(htmlView);
            }
            else
            {
                mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(mailItem.Body.RemoveHtmlTags(), new ContentType("text/plain; charset=UTF-8")));
            }

            if (mailItem.VCalendarView.HasValue())
            {
                var calendarType = new ContentType("text/calendar");
                calendarType.Parameters.Add("method", "REQUEST");
                calendarType.Parameters.Add("name", "meeting.ics");

                var calendarView = AlternateView.CreateAlternateViewFromString(mailItem.VCalendarView, calendarType);
                calendarView.TransferEncoding = TransferEncoding.SevenBit;

                mail.AlternateViews.Add(calendarView);
            }

            #endregion

            #region Set Sender

            mail.From = mailItem.GetSender();
            mail.ReplyToList.Add(mailItem.GetReplyTo());

            #endregion

            #region Set Receivers

            // Add To:
            foreach (var address in mailItem.To.Or("").Split(',').Trim().Where(a => IsSendingPermitted(a)))
                mail.To.Add(address);

            // Add Cc:
            foreach (var address in mailItem.Cc.Or("").Split(',').Trim().Where(a => IsSendingPermitted(a)))
                mail.CC.Add(address);

            foreach (var address in Config.Get("Email:Auto.CC.Address").Or("").Split(',').Trim().Where(a => IsSendingPermitted(a)))
                mail.CC.Add(address);

            // Add Bcc:
            foreach (var address in mailItem.Bcc.Or("").Split(',').Trim().Where(a => IsSendingPermitted(a)))
                mail.Bcc.Add(address);

            if (mail.To.None() && mail.CC.None() && mail.Bcc.None())
                return null;

            #endregion

            // Add attachments
            mail.Attachments.AddRange(await mailItem.GetAttachments());

            return mail;
        }

        public static MailAddress GetSender(this IEmailQueueItem mailItem)
        {
            var addressPart = mailItem.SenderAddress.Or(Config.Get("Email:Sender:Address"));
            var displayNamePart = mailItem.SenderName.Or(Config.Get("Email:Sender:Name"));
            return new MailAddress(addressPart, displayNamePart);
        }

        public static MailAddress GetReplyTo(this IEmailQueueItem mailItem)
        {
            var result = mailItem.GetSender();

            var asCustomReplyTo = mailItem as ICustomReplyToEmailQueueItem;
            if (asCustomReplyTo == null) return result;

            return new MailAddress(asCustomReplyTo.ReplyToAddress.Or(result.Address),
                    asCustomReplyTo.ReplyToName.Or(result.DisplayName));
        }

        #region Configuration

        /// <summary>
        /// Configures this smtp client with the specified config file path.
        /// </summary>
        public static void Configure(this SmtpClient client)
        {
            var setting = Config.Bind<SmtpNetworkSetting>("system.net:mailSettings");

            client.Port = setting.Port;

            if (setting.TargetName.HasValue())
                client.TargetName = setting.TargetName;

            if (client.DeliveryMethod == SmtpDeliveryMethod.Network)
                client.Host = setting.Host;

            if (setting.DefaultCredentials && setting.UserName.HasValue() &&
                 setting.Password.HasValue())
            {
                client.Credentials = new NetworkCredential(setting.UserName, setting.Password);
            }
        }

        #endregion

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