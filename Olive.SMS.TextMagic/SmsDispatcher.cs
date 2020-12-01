using System;
using System.Threading.Tasks;
using TextmagicRest;
using Olive;

namespace Olive.SMS.TextMagic
{
    public class SmsDispatcher : Olive.SMS.ISmsDispatcher
    {

        public Task Dispatch(ISmsMessage sms)
        {
            var client = new Client(Config.GetOrThrow("Sms:TextMagic:Username"), Config.GetOrThrow("Sms:TextMagic:Key"));
            var result = client.SendMessage(sms.Text, sms.To);

            if (!result.Success)
                throw new Exception("Failed to send message because : " + result.ClientException.Message);

            return Task.CompletedTask;
        }

    }
}
