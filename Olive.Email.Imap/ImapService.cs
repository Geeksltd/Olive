using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive.Email
{
    public class ImapService: IImapService
    {
        private readonly IConfiguration Configuration;

        public ImapService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<IEnumerable<MimeMessage>> GetNewMessage(string folder = "Inbox")
        {
            var result = new List<MimeMessage>();

            using (var client = await CreateClient())
            {
                var theFolder = await GetFolder(client, folder);

                foreach (var id in await theFolder.SearchAsync(SearchQuery.NotSeen))
                    result.Add(await theFolder.GetMessageAsync(id));
            }

            return result;
        }

        private async Task<IMailFolder> GetFolder(
            ImapClient client,
            string folder,
            FolderAccess access = FolderAccess.ReadOnly)
        {
            var result = await client.GetFolderAsync(folder);
            await result.OpenAsync(access);

            return result;
        }

        [EscapeGCop("It is internally used and will be disposed")]
        private async Task<ImapClient> CreateClient()
        {
            string host = Configuration.GetValue<string>("Email:ImapHost");
            int port = Configuration.GetValue<int>("Email:ImapPort");
            bool enableSsl = Configuration.GetValue<bool>("Email:EnableSsl");
            string username = Configuration.GetValue<string>("Email:Username");
            string password = Configuration.GetValue<string>("Email:Password");

            var result = new ImapClient();

            await result.ConnectAsync(host, port, enableSsl);
            await result.AuthenticateAsync(username, password);

            return result;
        }

        public async Task<IEnumerable<MimeMessage>> GetReferences(MimeMessage main)
        {
            var result = new List<MimeMessage>();

            using (var client = await CreateClient())
            {
                var folder = client.GetFolder(SpecialFolder.All);
                await folder.OpenAsync(FolderAccess.ReadWrite);

                foreach (var referenceId in main.References)
                {
                    var ids = await folder.SearchAsync(SearchQuery.HeaderContains("Message-Id", referenceId));
                    await ids.DoAsync(async (id, _) => result.Add(await folder.GetMessageAsync(id)));
                }
            }

            return result;
        }

        public async Task MarkAsSeen(MimeMessage message)
        {
            using (var client = await CreateClient())
            {
                var folder = client.GetFolder(SpecialFolder.All);
                await folder.OpenAsync(FolderAccess.ReadWrite);

                var ids = await folder.SearchAsync(SearchQuery.HeaderContains("Message-Id", message.MessageId));

                await ids.DoAsync((id, _) => folder.SetFlagsAsync(id, MessageFlags.Seen, silent: true));
            }
        }
    }
}
