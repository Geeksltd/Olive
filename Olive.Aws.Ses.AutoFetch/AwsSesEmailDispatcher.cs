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
            Context.Current.GetService<Entities.Data.IDatabaseProviderConfig>().RegisterDataProvider(typeof(MailMessage),
            new Entities.Data.DataProviderFactory(typeof(MailMessage)).GetProvider(typeof(MailMessage)));
            Watch<MailMessage>(emailS3Bucket);
        }

        public static void Watch<TMailMessage>(string emailS3Bucket) where TMailMessage : IMailMessage, new()
        {
            Accounts.Add(new EmailAccount<TMailMessage>(emailS3Bucket));
        }

        public static async Task FetchAll()
        {
            foreach (var account in Accounts)
            {
                try
                {
                    Log.For(typeof(Mailbox)).Info("Fetching emails for " + account.S3Bucket);
                    await FetchClient.Fetch(account);
                    Log.For(typeof(Mailbox)).Info("Fetched emails for " + account.S3Bucket);
                }
                catch (Exception ex)
                {
                    Log.For(typeof(Mailbox)).Error(ex, "Failed to fetch emails because " + ex.ToFullMessage());
                }
            }
        }
    }
}
