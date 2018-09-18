using Microsoft.Extensions.Logging;
using Olive.Entities;
using System;
using System.Threading.Tasks;

namespace Olive.SMS
{
    public class SmsService : ISmsService
    {
        readonly IDatabase Database;
        private readonly ILogger<SmsService> Log;

        /// <summary>
        /// Occurs when an exception happens when sending an sms. Sender parameter will be the ISmsQueueItem instance that couldn't be sent.
        /// </summary>
        public readonly AsyncEvent<SmsSendingEventArgs> SendError = new AsyncEvent<SmsSendingEventArgs>();

        public SmsService(IDatabase database, ILogger<SmsService> log)
        {
            Database = database;
            this.Log = log;
        }

        public async Task<bool> Send(ISmsQueueItem smsItem)
        {
            if (smsItem.Retries > Config.Get("SMS:MaximumRetries", 3))
                return false;
            try
            {
                ISMSSender sender;
                try
                {
                    sender = Activator.CreateInstance(Type.GetType(Config.GetOrThrow("SMS:SenderType"))) as ISMSSender;

                    if (sender == null)
                        throw new Exception("Type is not defined, or it does not implement ISMSSender");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Can not instantiate the sms sender from App config of " + Config.Get("SMS:SenderType"));
                    return false;
                }

                sender.Deliver(smsItem);

                await Database.Update(smsItem, o => o.DateSent = LocalTime.Now);
                return true;
            }
            catch (Exception ex)
            {
                await SendError.Raise(new SmsSendingEventArgs(smsItem) { Error = ex });
                Log.Error(ex, "Can not send the SMS queue item.");
                await RecordRetry(smsItem);
                return false;
            }
        }

        /// <summary>
        /// Records an unsuccessful attempt to send this SMS.
        /// </summary>
        public async Task RecordRetry(ISmsQueueItem sms)
        {
            if (sms.IsNew) throw new InvalidOperationException();

            await Database.Update(sms, s => s.Retries++);

            // Also update this local instance:
            sms.Retries++;
        }

        /// <summary>
        /// Updates the DateSent field of this item and then soft deletes it.
        /// </summary>
        public Task MarkSent(ISmsQueueItem sms)
        {
            return Database.EnlistOrCreateTransaction(() => Database.Update(sms, o => o.DateSent = LocalTime.Now));
        }

        public async Task SendAll()
        {
            foreach (var sms in await Database.GetList<ISmsQueueItem>(i => i.DateSent == null))
                await Send(sms);
        }
    }
}