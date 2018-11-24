using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Olive.Aws
{
    abstract class Agent<TMessage> where TMessage : IEventBusMessage
    {
        protected string QueueUrl;
        protected AmazonSQSClient Client;

        protected Agent()
        {
            var key = "Aws:QueueUrls:" + typeof(TMessage).FullName;
            QueueUrl = Config.Get(key).OrNullIfEmpty()
            ?? throw new Exception("Queue url not specified under " + key);

            Client = new AmazonSQSClient(Amazon.RegionEndpoint.EUWest1);
        }
    }
}
