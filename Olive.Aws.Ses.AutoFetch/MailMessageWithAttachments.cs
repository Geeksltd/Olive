namespace Olive.Aws.Ses.AutoFetch
{
    public class MailMessageWithAttachments
    {
        public IMailMessage Message { get; set; }
        public IMailMessageAttachment[] Attachments { get; set; }
    }
}