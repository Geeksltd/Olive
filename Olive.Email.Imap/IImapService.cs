using MimeKit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olive.Email
{
    public interface IImapService
    {
        Task<IEnumerable<MimeMessage>> GetNewMessage(string folder = "Inbox");
        Task<IEnumerable<MimeMessage>> GetReferences(MimeMessage main);
        Task MarkAsSeen(MimeMessage message);
    }
}