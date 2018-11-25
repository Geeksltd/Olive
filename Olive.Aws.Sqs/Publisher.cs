using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Olive.Aws
{
    class Publisher : Agent
    {
        SendMessageRequest Request;

        public Publisher(string queueKey) : base(queueKey)
        {
        }

        public async Task<string> Publish(IEventBusMessage message)
        {
            Request = new SendMessageRequest
            {
                QueueUrl = QueueUrl,
                MessageBody = JsonConvert.SerializeObject(message),
                MessageDeduplicationId = message.DeduplicationId,
                MessageGroupId = "Default"
            };

            var response = await Client.SendMessageAsync(Request);
            return response.MessageId;
        }
    }
}
