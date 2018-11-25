using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Olive.Aws
{
    abstract class Agent
    {
        protected string QueueUrl;
        protected AmazonSQSClient Client;
        string QueueKey;

        protected Agent(string queueKey)
        {
            QueueKey = queueKey;
            var key = "Aws:QueueUrls:" + queueKey;

            QueueUrl = Config.Get(key).OrNullIfEmpty()

            ?? throw new Exception("Queue url not specified under " + key);

            Client = new AmazonSQSClient(Amazon.RegionEndpoint.EUWest1);
        }
    }
}
