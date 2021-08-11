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
    public class EmailOutbox : IEmailOutbox
    {
        static Random Random = new Random();
        AsyncLock AsyncLock = new AsyncLock();
        ILogger<EmailOutbox> Log;

        public event AwaitableEventHandler<EmailSendingEventArgs> Sending;
        public event AwaitableEventHandler<EmailSendingEventArgs> Sent;
        public event AwaitableEventHandler<EmailSendingEventArgs> SendError;
        public IEmailDispatcher Dispatcher { get; }
        public IMailMessageCreator MessageCreator { get; }
        public IEmailRepository Repository { get; }

        public EmailOutbox(IEmailDispatcher dispatcher,
            IMailMessageCreator messageCreator,
            IEmailRepository repository,
            ILogger<EmailOutbox> log)
        {
            Dispatcher = dispatcher;
            MessageCreator = messageCreator;
            Repository = repository;
            Log = log;
        }

        public async Task SendAll(TimeSpan? delayPerSend = null)
        {
            Log.Info("Sending all ...");
            using (await AsyncLock.Lock())
            {
                var toSend = await Repository.GetUnsentEmails();

                Log.Info($"Loaded {toSend.Count()} emails to send ...");

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

                    try
                    {
                        Log.Info($"Sending {mail.GetId()?.ToString().Or(mail.To.Substring(3))} ...");
                        await Send(mail);
                        Log.Info($"Sent {mail.GetId()?.ToString().Or(mail.To.Substring(3))} ...");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Could not send a queued email message " + mail.GetId() + " because " + ex.ToFullMessage());
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
                await Repository.SaveForFutureSend(message);

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
                    await Dispatcher.Dispatch(mail, message);

                    await Repository.RecordEmailSent(message);
                    await Sent.Raise(new EmailSendingEventArgs(message, mail));

                    return true;
                }
                catch (Exception ex)
                {
                    await SendError.Raise(new EmailSendingEventArgs(message, mail) { Error = ex });
                    await Repository.RecordRetry(message);
                    Log.Error(ex, $"Error in sending an email for this EmailQueueItem of '{message.GetId()}' because : " + Environment.NewLine + ex.ToFullMessage());
                    return false;
                }
            }
        }

        public async Task<IEnumerable<T>> GetSentEmails<T>() where T : IEmailMessage
            => await Repository.GetSentEmails<T>();
    }
}