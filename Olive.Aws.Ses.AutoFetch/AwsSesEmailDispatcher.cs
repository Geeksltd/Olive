using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive.Aws.Ses.AutoFetch
{
    public class Mailbox
    {
        static List<EmailAccount> Accounts = new List<EmailAccount>();
        public static async Task Watch(string emailS3Bucket)
        {
            await DatabaseTableService.EnsureDatabaseTable<MailMessage, MailMessageAttachment>();

            Context.Current
                .GetService<Entities.Data.IDatabaseProviderConfig>()
                .RegisterDataProvider(typeof(MailMessage),
            new Entities.Data.DataProviderFactory(typeof(MailMessage)).GetProvider(typeof(MailMessage)));

            Context.Current
                .GetService<Entities.Data.IDatabaseProviderConfig>()
                .RegisterDataProvider(typeof(MailMessageAttachment),
            new Entities.Data.DataProviderFactory(typeof(MailMessageAttachment)).GetProvider(typeof(MailMessageAttachment)));

            Watch<MailMessage, MailMessageAttachment>(emailS3Bucket);
        }

        public static void Watch<TMailMessage, TMailMessageAttachment>(string emailS3Bucket)
            where TMailMessage : IMailMessage, new()
            where TMailMessageAttachment : IMailMessageAttachment, new()
        {
            Accounts.Add(new EmailAccount<TMailMessage, TMailMessageAttachment>(emailS3Bucket));
        }

        public static Task FetchAll<TMailMessage, TMailMessageAttachment>(string emailS3Bucket, Func<IMailMessage, Task<IMailMessage>> saveMessage = null, Func<IMailMessageAttachment[], Task> saveAttachments = null)
            where TMailMessage : IMailMessage, new()
            where TMailMessageAttachment : IMailMessageAttachment, new()
            => FetchAll(i => saveMessage((TMailMessage)i), i => saveAttachments(i), new EmailAccount<TMailMessage, TMailMessageAttachment>(emailS3Bucket));

        static async Task FetchAll(Func<IMailMessage, Task<IMailMessage>> saveMessage = null, Func<IMailMessageAttachment[], Task> saveAttachments = null, params EmailAccount[] accounts)
        {
            foreach (var account in accounts)
            {
                Log.For(typeof(Mailbox)).Info("Fetching emails for " + account.S3Bucket);
                await FetchClient.Fetch(account, saveMessage, saveAttachments);
                Log.For(typeof(Mailbox)).Info("Fetched emails for " + account.S3Bucket);
            }
        }
        public static Task FetchAll() => FetchAll(null, null, Accounts.ToArray());
    }
}