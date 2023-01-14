using Amazon.SimpleSystemsManagement;
using model = Amazon.SimpleSystemsManagement.Model;
using System.Threading.Tasks;
using System;

namespace Olive.Aws.Providers
{
    class SystemsManagerParameter : AwsSecretProvider<AmazonSimpleSystemsManagementClient>
    {
        internal override async Task<string> Download(string secretId)
        {
            if (secretId.IsEmpty())
            {
                Log.For(this).Error("Secret Id Is Empty");
                throw new ArgumentNullException(nameof(secretId));
            }

            var request = new model.GetParameterRequest { Name = secretId };
            var response = await AwsClient.GetParameterAsync(request).ConfigureAwait(false);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                Log.For(this).Error($"Secret Id Response : {response.HttpStatusCode} - {response.Parameter.Name}");

            return response.Parameter.Value;
        }
    }
}
