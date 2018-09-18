using System.Threading.Tasks;

namespace Olive.SMS
{
    public interface ISmsService
    {
        /// <summary>
        /// Sends the specified SMS item.
        /// It will try several times to deliver the message. The number of retries can be specified in AppConfig of "SMS:MaximumRetries".
        /// If it is not declared in web.config, then 3 retires will be used.
        /// Note: The actual SMS Sender component must be implemented as a public type that implements ISMSSender interface.
        /// The assembly qualified name of that component, must be specified in AppConfig of "SMS:SenderType".
        /// </summary>
        Task<bool> Send(ISmsMessage smsItem);

        Task SendAll();
    }
}
