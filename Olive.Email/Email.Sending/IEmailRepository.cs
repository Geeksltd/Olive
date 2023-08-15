using Microsoft.Extensions.Configuration;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Email
{
    public interface IEmailRepository
    {
        /// <summary>Gets all emails ready to be sent.</summary>
        Task<IEnumerable<IEmailMessage>> GetUnsentEmails();

        /// <summary>Gets all emails that have been sent</summary>
        Task<IEnumerable<T>> GetSentEmails<T>() where T : IEmailMessage;

        /// <summary>Performs actions required to record the email as sent.</summary>
        Task RecordEmailSent(IEmailMessage message);

        /// <summary>Performs actions required to record the retries on an email that failed to send.</summary>
        Task RecordRetry(IEmailMessage message);

        /// <summary>Performs actions required to save the email for sending in the future.</summary>
        Task SaveForFutureSend(IEmailMessage message);
    }
}