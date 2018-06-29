using System;
using System.Threading.Tasks;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Olive.Aws;

namespace Olive.Security.Aws.Services
{
    class DataKeyService
    {
        static readonly string MasterKeyArn = Config.Get("Aws:Kms:MasterKeyArn")
            .Or(Environment.GetEnvironmentVariable("AWS_KMS_MASTERKEY_ARN"))
            .OrNullIfEmpty() ?? throw new Exception("Aws Master Key Arn is not specified.");

        static AmazonKeyManagementServiceClient CreateClient()
            => new AmazonKeyManagementServiceClient(RuntimeIdentity.Credentials, RuntimeIdentity.Region);

        internal static async Task<Key> GenerateKey()
        {
            using (var kms = CreateClient())
            {
                var dataKey = await kms.GenerateDataKeyAsync(new GenerateDataKeyRequest
                {
                    KeyId = MasterKeyArn,
                    KeySpec = DataKeySpec.AES_256
                });

                return new Key
                {
                    Cipher = dataKey.CiphertextBlob.ReadAllBytes(),
                    Plain = dataKey.Plaintext.ReadAllBytes()
                };
            }
        }

        internal static async Task<byte[]> GetPlainKey(byte[] cipher)
        {
            using (var kms = CreateClient())
            {
                var decryptedData = await kms.DecryptAsync(new DecryptRequest
                {
                    CiphertextBlob = cipher.AsStream()
                });

                return decryptedData.Plaintext.ReadAllBytes();
            }
        }

        internal class Key
        {
            public byte[] Cipher { get; set; }
            public byte[] Plain { get; set; }
        }
    }
}
