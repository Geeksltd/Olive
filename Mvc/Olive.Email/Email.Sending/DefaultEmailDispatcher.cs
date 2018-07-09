using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Olive.Email
{
    public class DefaultEmailDispatcher : IEmailDispatcher
    {
        /// <summary>
        /// Provides a message which can dispatch an email message.
        /// Returns whether the message was sent successfully.
        /// </summary>
        public async Task<bool> Dispatch(IEmailMessage mailItem, MailMessage mail)
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.EnableSsl = mailItem.EnableSsl ?? Config.GetOrThrow("Email:EnableSsl").To<bool>();
                smtpClient.Port = mailItem.SmtpPort ?? Config.GetOrThrow("Email:SmtpPort").To<int>();
                smtpClient.Host = mailItem.SmtpHost.OrNullIfEmpty() ?? Config.GetOrThrow("Email:SmtpHost");

                var userName = mailItem.Username.OrNullIfEmpty() ?? Config.GetOrThrow("Email:Username");
                var password = mailItem.Password.OrNullIfEmpty() ?? Config.GetOrThrow("Email:Password");
                smtpClient.Credentials = new NetworkCredential(userName, password);

                await smtpClient.SendMailAsync(mail);

                if (!mailItem.IsNew) await Context.Current.Database().Delete(mailItem);
            }

            return true;
        }
    }
}