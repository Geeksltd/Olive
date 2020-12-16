using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Configuration;

namespace Olive.SMS
{
    public class SmsDispatcher : ISmsDispatcher, IDisposable
    {
        readonly AmazonSimpleNotificationServiceClient Client;
        readonly IConfiguration Configuration;

        public SmsDispatcher(IConfiguration configuration)
        {
            Client = new AmazonSimpleNotificationServiceClient();
            Configuration = configuration;
        }

        public async Task Dispatch(ISmsMessage sms)
        {
            var senderId = sms.SenderName ?? Configuration.GetValue<string>("Aws:Sns:SenderId");

            var messageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "AWS.SNS.SMS.SenderID",
                    new MessageAttributeValue { StringValue = senderId, DataType = "String" }
                },

                {
                    "AWS.SNS.SMS.SMSType",
                    new MessageAttributeValue { DataType = "String", StringValue = "Transactional" }
                }
            };

            var pubRequest = new PublishRequest
            {
                Message = sms.Text,
                PhoneNumber = sms.To,
                MessageAttributes = messageAttributes,
            };

            await Client.PublishAsync(pubRequest);
        }

        public void Dispose() => Client.Dispose();
    }
}
