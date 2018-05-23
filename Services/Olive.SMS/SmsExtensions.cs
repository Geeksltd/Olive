using System;
using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.SMS
{
    public static class SmsExtensions
    {
        static IDatabase Database => Context.Current.Database();

        /// <summary>
        /// Records an unsuccessful attempt to send this SMS.
        /// </summary>
        public static async Task RecordRetry(this ISmsQueueItem sms)
        {
            if (sms.IsNew) throw new InvalidOperationException();

            await Database.Update(sms, s => s.Retries++);

            // Also update this local instance:
            sms.Retries++;
        }

        /// <summary>
        /// Updates the DateSent field of this item and then soft deletes it.
        /// </summary>
        public static Task MarkSent(this ISmsQueueItem sms)
        {
            return Database.EnlistOrCreateTransaction(() => Database.Update(sms, o => o.DateSent = LocalTime.Now));
        }

        /// <summary>
        /// Sends the specified SMS item.
        /// It will try several times to deliver the message. The number of retries can be specified in AppConfig of "SMS:MaximumRetries".
        /// If it is not declared in web.config, then 3 retires will be used.
        /// Note: The actual SMS Sender component must be implemented as a public type that implements ISMSSender interface.
        /// The assembly qualified name of that component, must be specified in AppConfig of "SMS:SenderType".
        /// </summary>
        public static Task<bool> Send(this ISmsQueueItem sms) => SmsService.Send(sms);
    }
}