using Microsoft.Extensions.Logging;
using MimeKit;
using Olive.Entities;
using System;
using System.Threading.Tasks;
using static Olive.Email.Constants;

namespace Olive.Email
{
    public abstract class BaseEmailFailureService : IEmailFailureService
    {
        protected readonly IImapService ImapService;
        protected readonly IDatabase Database;
        protected readonly ILogger Logger;

        public BaseEmailFailureService(
            IImapService imapService,
            IDatabase database,
            ILogger<BaseEmailFailureService> logger
            )
        {
            ImapService = imapService;
            Logger = logger;
            Database = database;
        }

        public async Task Check()
        {
            var newEmails = await ImapService.GetNewMessage();

            foreach (var email in newEmails)
            {
                if (IsFailure(email))
                {
                    Logger.LogInformation($"Recieved a failure email with id '{email.MessageId}' and subject '{email.Subject}'");

                    using (var scope = Database.CreateTransactionScope())
                    {
                        await MarkMessageItemAsFailure(email);
                        scope.Complete();
                    }

                    await ImapService.MarkAsSeen(email);
                }
            }
        }

        protected virtual async Task MarkMessageItemAsFailure(MimeMessage mainEmail)
        {
            var references = await ImapService.GetReferences(mainEmail);

            foreach (var email in references)
            {
                if (email.Headers.Contains(EMAIL_MESSAGE_ID_HEADER_KEY))
                {
                    var emailItem = await Database.Get<IEmailMessage>(email.Headers[EMAIL_MESSAGE_ID_HEADER_KEY]);
                    await MarkMessageItemAsFailure(emailItem);
                }
            }
        }

        protected abstract Task MarkMessageItemAsFailure(IEmailMessage emailMessage);

        protected virtual bool IsFailure(MimeMessage email) =>
            email.Subject.Contains("failure", StringComparison.CurrentCultureIgnoreCase);
    }
}