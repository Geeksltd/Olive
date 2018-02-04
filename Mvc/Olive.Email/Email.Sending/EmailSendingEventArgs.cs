namespace Olive.Email
{
    using System;
    using System.Net.Mail;

    public class EmailSendingEventArgs
    {
        public MailMessage MailMessage { get; }
        public IEmailMessage Item { get; }

        public Exception Error { get; internal set; }

        public EmailSendingEventArgs(IEmailMessage item, MailMessage message)
        {
            MailMessage = message;
            Item = item;
        }
    }
}
