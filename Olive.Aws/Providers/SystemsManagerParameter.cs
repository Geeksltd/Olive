using Amazon.SimpleSystemsManagement;
using model = Amazon.SimpleSystemsManagement.Model;
using System.Threading.Tasks;

namespace Olive.Aws.Providers
{
    class SystemsManagerParameter : AwsSecretProvider<AmazonSimpleSystemsManagementClient>
    {
        internal override async Task<string> Download(string secretId)
        {
            var request = new model.GetParameterRequest { Name = secretId };
            var response = await AwsClient.GetParameterAsync(request);
            return response.Parameter.Value;
        }
    }
}
