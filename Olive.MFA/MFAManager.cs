namespace Olive.MFA
{
    using Olive.Email;
    using Olive.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Twilio;
    using Twilio.Rest.Api.V2010.Account;
    using Twilio.Types;

    public class MFAManager : IMFAManager
    {
        IDatabase Database => Context.Current.Database();
        public Task SendOtpViaEmail(IEmailMessage emailMessage)
        {
            return Database.Save(emailMessage);
        }

        public Task<bool> SendOtpViaSms(string smsMessage, string phoneNumber)
        {
            var accountId = Config.Get("TwilioAccountId");
            var authKey = Config.Get("TwilioAuthKey");
            var sender = Config.Get("TwilioSenderNumber");

            if (accountId.IsEmpty() || authKey.IsEmpty() || sender.IsEmpty())
                throw new Exception("TwilioAccountId | TwilioAuthKey | TwilioSenderNumber configuration not set");



            TwilioClient.Init(accountId, authKey);
            var message = MessageResource.Create(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(sender),
                body: smsMessage);

            return Task.FromResult(message != null);
        }
        public string GetOtp()
        {
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();
            return otp;

        }
    }
}
