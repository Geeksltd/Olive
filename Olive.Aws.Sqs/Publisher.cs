using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Olive.Aws
{
    class Publisher<TMessage> : Agent<TMessage> where TMessage : IEventBusMessage
    {
        SendMessageRequest Request;

        public async Task<string> Publish(TMessage message)
        {
            Request = new SendMessageRequest
            {
                QueueUrl = QueueUrl,
                MessageBody = JsonConvert.SerializeObject(message),
                MessageDeduplicationId = message.DeduplicationId,
                MessageGroupId = typeof(TMessage).FullName
            };

            var response = await Client.SendMessageAsync(Request);
            return response.MessageId;
        }
    }
}
