using System.Net.Mail;
using System.Threading.Tasks;

namespace Olive.Email
{
    public interface IEmailDispatcher
    {
        /// <summary>
        /// Provides a message which can dispatch an email message.
        /// Returns whether the message was sent successfully.
        /// </summary>
        Task<bool> Dispatch(IEmailMessage mailItem, MailMessage mail);
    }
}