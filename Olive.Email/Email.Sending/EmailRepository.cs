using Microsoft.Extensions.Configuration;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Email
{
    public class EmailRepository : IEmailRepository
    {
        static IDatabase Database => Context.Current.Database();
        readonly EmailConfiguration Config;

        public EmailRepository(IConfiguration config)
        {
            Config = config.GetSection("Email").Get<EmailConfiguration>();
        }

        public async Task<IEnumerable<IEmailMessage>> GetUnsentEmails()
        {
            var records = await Database.Of<IEmailMessage>().GetList();
            var result = records
                .Where(x => x.Retries < Config.MaxRetries)
                .OrderBy(x => x.SendableDate);

            return result;
        }

        public async Task<IEnumerable<T>> GetSentEmails<T>() where T : IEmailMessage
        {
            using (new SoftDeleteAttribute.Context(bypassSoftdelete: false))
            {
                var records = await Database.GetList<T>();
                var result = records.OfType<Entity>().Where(x => SoftDeleteAttribute.IsMarked(x));
                return result.Cast<T>();
            }
        }

        public async Task RecordEmailSent(IEmailMessage message)
        {
            if (!message.IsNew) await Database.Delete(message);
        }

        public async Task RecordRetry(IEmailMessage message)
        {
            var retries = message.Retries + 1;

            if (!message.IsNew)
            {
                await Database.Update(message, e => e.Retries = retries);
                // Also update this local instance:
                message.Retries = retries;
            }
            else
            {
                message.Retries += 1;
                await Database.Save(message);
            }
        }

        public async Task SaveForFutureSend(IEmailMessage message)
        {
            if (message.IsNew) await Database.Save(message);
        }
    }
}