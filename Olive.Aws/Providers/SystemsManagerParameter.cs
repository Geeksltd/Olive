using Amazon.SimpleSystemsManagement;
using model = Amazon.SimpleSystemsManagement.Model;
using System.Threading.Tasks;
using System;
using System.Reflection;

namespace Olive.Aws.Providers
{
    class SystemsManagerParameter : AwsSecretProvider<AmazonSimpleSystemsManagementClient>
    {
        internal override string Download(string secretId)
        {
            if (secretId.IsEmpty())
            {
                Console.WriteLine("ERROR: Secret Id Is Empty");
                throw new ArgumentNullException(nameof(secretId));
            }

            var request = new model.GetParameterRequest { Name = secretId };

            var response = (model.GetParameterResponse)AwsClient.GetType().GetMethod("GetParameter", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(AwsClient, new object[] { request });

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                Console.WriteLine($"Secret Id Response : {response.HttpStatusCode} - {response.Parameter.Name}");

            return response.Parameter.Value;
        }
    }
}