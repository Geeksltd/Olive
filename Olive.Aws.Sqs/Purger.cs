using Amazon.SQS.Model;
using System.Threading.Tasks;

namespace Olive.Aws
{
    class Purger : Agent
    {
        public Purger(string queueKey) : base(queueKey) { }

        public Task Run()
        {
            return Client.PurgeQueueAsync(new PurgeQueueRequest { QueueUrl = QueueUrl });
            // Should we check the response status?
        }
    }
}