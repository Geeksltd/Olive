using System;
using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.SMS
{
    public static class SmsService
    {
        static IDatabase Database => Context.Current.Database();

        /// <summary>
        /// Occurs when an exception happens when sending an sms. Sender parameter will be the ISmsQueueItem instance that couldn't be sent.
        /// </summary>
        public static readonly AsyncEvent<SmsSendingEventArgs> SendError = new AsyncEvent<SmsSendingEventArgs>();

        /// <summary>
        /// Sends the specified SMS item.
        /// It will try several times to deliver the message. The number of retries can be specified in AppConfig of "SMS:MaximumRetries".
        /// If it is not declared in web.config, then 3 retires will be used.
        /// Note: The actual SMS Sender component must be implemented as a public type that implements ISMSSender interface.
        /// The assembly qualified name of that component, must be specified in AppConfig of "SMS:SenderType".
        /// </summary>
        public static async Task<bool> Send(ISmsQueueItem smsItem)
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
                await smsItem.RecordRetry();
                return false;
            }
        }

        public static async Task SendAll()
        {
            foreach (var sms in await Database.GetList<ISmsQueueItem>(i => i.DateSent == null))
                await sms.Send();
        }
    }
}