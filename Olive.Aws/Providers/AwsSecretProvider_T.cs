using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.Aws.Providers
{
    abstract class AwsSecretProvider<TAwsClient> : AwsSecretProvider where TAwsClient : AmazonServiceClient, new()
    {
        protected TAwsClient AwsClient { get; private set; }

        public AwsSecretProvider()
        {
            AwsClient = new TAwsClient();
        }
    }
}
