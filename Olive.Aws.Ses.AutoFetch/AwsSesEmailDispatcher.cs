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
            await DatabaseTableService.EnsureDatabaseTable<MailMessage>();

            Context.Current
                .GetService<Entities.Data.IDatabaseProviderConfig>()
                .RegisterDataProvider(typeof(MailMessage),
            new Entities.Data.DataProviderFactory(typeof(MailMessage)).GetProvider(typeof(MailMessage)));

            Watch<MailMessage>(emailS3Bucket);
        }

        public static void Watch<TMailMessage>(string emailS3Bucket) where TMailMessage : IMailMessage, new()
        {
            Accounts.Add(new EmailAccount<TMailMessage>(emailS3Bucket));
        }

        public static Task FetchAll<TMailMessage>(string emailS3Bucket, Func<TMailMessage, Task> save = null)
            where TMailMessage : IMailMessage, new()
            => FetchAll(i => save((TMailMessage)i), new EmailAccount<TMailMessage>(emailS3Bucket));

        static async Task FetchAll(Func<IMailMessage, Task> save = null, params EmailAccount[] accounts)
        {
            foreach (var account in accounts)
            {
                Log.For(typeof(Mailbox)).Info("Fetching emails for " + account.S3Bucket);
                await FetchClient.Fetch(account, save);
                Log.For(typeof(Mailbox)).Info("Fetched emails for " + account.S3Bucket);
            }
        }
        public static Task FetchAll() => FetchAll(null, Accounts.ToArray());
    }
}