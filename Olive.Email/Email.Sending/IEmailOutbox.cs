using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive.Email
{
    public interface IEmailOutbox
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
        event AwaitableEventHandler<EmailSendingEventArgs> Sending;

        /// <summary>
        /// Occurs when the smtp mail message for this email is sent.
        /// Sender is the IEmailMessage instance that was sent.
        /// </summary>
        event AwaitableEventHandler<EmailSendingEventArgs> Sent;

        /// <summary>
        /// Occurs when an exception happens when sending an email.
        /// Sender parameter will be the IEmailMessage instance that couldn't be sent.
        /// </summary>
        event AwaitableEventHandler<EmailSendingEventArgs> SendError;

        /// <summary>
        /// Gets the email items which have been sent (marked as soft deleted).
        /// </summary>
        Task<IEnumerable<T>> GetSentEmails<T>() where T : IEmailMessage;
    }
}