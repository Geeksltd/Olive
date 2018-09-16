using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Email
{
    public interface IEmailSender
    {
        /// <summary>Tries to sends all unsent emails from the queue.</summary>
        Task SendAll(TimeSpan? delayPerSend = null);

        /// <summary>
        /// Will try to send the specified email and returns true for successful sending.
        /// </summary>
        Task<bool> Send(IEmailMessage message);

        /// <summary>
        /// Occurs when the smtp mail message for this email is about to be sent.
        /// </summary>
        AsyncEvent<EmailSendingEventArgs> Sending { get; }

        /// <summary>
        /// Occurs when the smtp mail message for this email is sent.
        /// Sender is the IEmailMessage instance that was sent.
        /// </summary>
        AsyncEvent<EmailSendingEventArgs> Sent { get; }

        /// <summary>
        /// Occurs when an exception happens when sending an email.
        /// Sender parameter will be the IEmailMessage instance that couldn't be sent.
        /// </summary>
        AsyncEvent<EmailSendingEventArgs> SendError { get; }

        /// <summary>
        /// Gets the email items which have been sent (marked as soft deleted).
        /// </summary>
        Task<IEnumerable<T>> GetSentEmails<T>() where T : IEmailMessage;
    }

    public class EmailSender : IEmailSender
    {
        static Random Random = new Random();
        AsyncLock AsyncLock = new AsyncLock();
        IDatabase Database;
        EmailConfiguration Config;
        int MaximumRetries;
        ILogger<EmailSender> Log;

        public AsyncEvent<EmailSendingEventArgs> Sending { get; }
        public AsyncEvent<EmailSendingEventArgs> Sent { get; }
        public AsyncEvent<EmailSendingEventArgs> SendError { get; }
        public IEmailDispatcher Dispatcher { get; }
        public IMailMessageCreator MessageCreator { get; }

        public EmailSender(IEmailDispatcher dispatcher,
            IMailMessageCreator messageCreator,
            IDatabase database, IConfiguration config, ILogger<EmailSender> log)
        {
            Dispatcher = dispatcher;
            MessageCreator = messageCreator;
            Database = database;
            Config = config.GetSection("Email").Get<EmailConfiguration>();
            Log = log;
            Sending = new AsyncEvent<EmailSendingEventArgs>();
            Sent = new AsyncEvent<EmailSendingEventArgs>();
            SendError = new AsyncEvent<EmailSendingEventArgs>();
        }

        Task<IEmailMessage[]> GetUnsentEmails()
        {
            return Database.Of<IEmailMessage>()
                .Where(x => x.Retries < MaximumRetries)
                  .OrderBy(e => e.SendableDate)
                  .GetList()
                  .ToArray();
        }

        public async Task SendAll(TimeSpan? delayPerSend = null)
        {
            using (await AsyncLock.Lock())
            {
                var toSend = await GetUnsentEmails();

                foreach (var mail in toSend)
                {
                    if (delayPerSend > TimeSpan.Zero)
                    {
                        var multiply = 1 + (Random.NextDouble() - 0.5) / 4; // from 0.8 to 1.2

                        try { await Task.Delay(delayPerSend.Value.Multiply(multiply)); }
                        catch (ThreadAbortException)
                        {
                            return; // Application terminated.
                        }
                    }

                    try { await Send(mail); }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Could not send a queued email message " + mail.GetId());
                    }
                }
            }
        }

        public async Task<bool> Send(IEmailMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (message.SendableDate > LocalTime.Now)
            {
                Log.Info($"Skipping Send() command for IEmailMessage ({message.GetId()}). SendableDate is in the future.");
                return false;
            }

            using (var mail = await MessageCreator.Create(message))
            {
                if (mail.To.None() && mail.CC.None() && mail.Bcc.None())
                {
                    Log.Info($"Mail message {message.GetId()} will not be sent as there is no effective recipient.");
                    return false;
                }

                try
                {
                    await Sending.Raise(new EmailSendingEventArgs(message, mail));
                    var result = await Dispatcher.Dispatch(message, mail);
                    await Sent.Raise(new EmailSendingEventArgs(message, mail));
                    return result;
                }
                catch (Exception ex)
                {
                    await SendError.Raise(new EmailSendingEventArgs(message, mail) { Error = ex });
                    await RecordRetry(message);
                    Log.Error(ex, $"Error in sending an email for this EmailQueueItem of '{message.GetId()}'");
                    return false;
                }
            }
        }

        async Task RecordRetry(IEmailMessage message)
        {
            var retries = message.Retries + 1;

            if (!message.IsNew)
                await Database.Update(message, e => e.Retries = retries);

            // Also update this local instance:
            message.Retries = retries;
        }

        public async Task<IEnumerable<T>> GetSentEmails<T>() where T : IEmailMessage
        {
            using (new SoftDeleteAttribute.Context(bypassSoftdelete: false))
            {
                var records = await Database.GetList<T>();
                var result = records.OfType<Entity>().Where(x => SoftDeleteAttribute.IsMarked(x));
                return result.Cast<T>();
            }
        }
    }
}