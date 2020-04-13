using Microsoft.Extensions.Logging;
using Olive.Entities;
using System;
using System.Threading.Tasks;

namespace Olive.SMS
{
    public class SmsService : ISmsService
    {
        readonly IDatabase Database;
        readonly ILogger<SmsService> Log;
        ISmsDispatcher Dispatcher;
        int MaximumRetries;

        /// <summary>
        /// Occurs when an exception happens when sending an sms. Sender parameter will be the ISmsMessage instance that couldn't be sent.
        /// </summary>
        public event AwaitableEventHandler<SmsSendingEventArgs> SendError;

        public SmsService(IDatabase database, ISmsDispatcher dispatcher, ILogger<SmsService> log)
        {
            Database = database;
            Dispatcher = dispatcher;
            Log = log;
            MaximumRetries = Config.Get("SMS:MaximumRetries", 3);
        }

        public async Task<bool> Send(ISmsMessage smsItem)
        {
            if (smsItem.Retries > MaximumRetries)
                return false;
            if (smsItem.IsNew)
                await Database.Save(smsItem);
            try
            {

                await Dispatcher.Dispatch(smsItem);
                await Database.Update(smsItem, o => o.DateSent = LocalTime.Now);
                return true;
            }
            catch (Exception ex)
            {
                await SendError.Raise(new SmsSendingEventArgs(smsItem) { Error = ex });
                Log.Error(ex, "Can not send the SMS queue item.");
                await RecordFailedAttempt(smsItem);
                return false;
            }
        }

        /// <summary>
        /// Records an unsuccessful attempt to send this SMS.
        /// </summary>
        public async Task RecordFailedAttempt(ISmsMessage sms)
        {
            if (!sms.IsNew)
                await Database.Update(sms, s => s.Retries++);

            // Also update this local instance:
            sms.Retries++;
        }

        /// <summary>
        /// Updates the DateSent field of this item and then soft deletes it.
        /// </summary>
        public Task MarkSent(ISmsMessage sms)
        {
            return Database.EnlistOrCreateTransaction(() => Database.Update(sms, o => o.DateSent = LocalTime.Now));
        }

        public async Task SendAll()
        {
            foreach (var sms in await Database.GetList<ISmsMessage>(i => i.DateSent == null))
                await Send(sms);
        }
    }
}