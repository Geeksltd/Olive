using Microsoft.Extensions.Configuration;
using System.Net;
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

    public class EmailDispatcher : IEmailDispatcher
    {
        EmailConfiguration Config;

        public EmailDispatcher(IConfiguration config)
        {
            Config = config.GetSection("Email").Get<EmailConfiguration>();
        }

        /// <summary>
        /// Provides a message which can dispatch an email message.
        /// Returns whether the message was sent successfully.
        /// </summary>
        public async Task<bool> Dispatch(IEmailMessage mailItem, MailMessage mail)
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.EnableSsl = Config.EnableSsl;
                smtpClient.Port = Config.SmtpPort;
                smtpClient.Host = Config.SmtpHost;

                smtpClient.Credentials = new NetworkCredential(Config.Username, Config.Password);

                await smtpClient.SendMailAsync(mail);

                if (!mailItem.IsNew) await Context.Current.Database().Delete(mailItem);
            }

            return true;
        }
    }
}