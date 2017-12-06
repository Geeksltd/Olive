namespace Olive.Services.Email
{
    public class EmailSendingEventArgs
    {
        public MailMessage MailMessage { get; }
        public IEmailQueueItem Item { get; }

        public Exception Error { get; internal set; }

        public EmailSendingEventArgs(IEmailQueueItem item, MailMessage message)
        {
            MailMessage = message;
            Item = item;
        }
    }
}
