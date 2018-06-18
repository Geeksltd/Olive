using System.IO;
using System.Threading.Tasks;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;

namespace Olive.Security.Aws.Services
{
    class DataKeyService
    {
        static readonly string MasterKeyArn = Config.GetOrThrow("Aws:Kms:MasterKeyArn");

        internal static async Task<Key> GenerateKey()
        {
            using (var kms = new AmazonKeyManagementServiceClient())
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
            using (var kms = new AmazonKeyManagementServiceClient())
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
