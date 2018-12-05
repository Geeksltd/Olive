using Amazon.SQS;
using System;

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
            QueueUrl = Config.GetOrThrow(QueueKey);
            Client = new AmazonSQSClient();
        }
    }
}
