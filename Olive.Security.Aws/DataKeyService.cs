using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Olive.Security.Cloud;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Olive.Security.Aws
{
    public class DataKeyService : IDataKeyService
    {
        readonly static ConcurrentDictionary<string, byte[]> EncryptionKeys = new ConcurrentDictionary<string, byte[]>();
        internal string MasterKeyArn;


        static AmazonKeyManagementServiceClient CreateClient()
            => new AmazonKeyManagementServiceClient();

        public async Task<Key> GenerateKey()
        {
            using (var kms = CreateClient())
            {
                var dataKey = await kms.GenerateDataKeyAsync(new GenerateDataKeyRequest
                {
                    KeyId = GetKeyIdOrThrow(),
                    KeySpec = DataKeySpec.AES_256
                });

                return new Key
                {
                    EncryptionKeyReference = dataKey.CiphertextBlob.ReadAllBytes(),
                    EncryptionKey = dataKey.Plaintext.ReadAllBytes()
                };
            }
        }

        private string GetKeyIdOrThrow() => MasterKeyArn.OrNullIfEmpty() ?? throw new Exception("Aws Master Key Arn is not specified.");

        public byte[] GetEncryptionKey(byte[] encryptionKeyReference)
        {
            var keyRef = encryptionKeyReference.ToBase64String();

            return EncryptionKeys.GetOrAdd(keyRef,
                x => Task.Factory.RunSync(() => DownloadEncryptionKey(encryptionKeyReference)));
        }

        async Task<byte[]> DownloadEncryptionKey(byte[] encryptionKeyReference)
        {
                using (var kms = CreateClient())
                {
                    var decryptedData = await kms.DecryptAsync(new DecryptRequest
                    {
                        CiphertextBlob = encryptionKeyReference.AsStream(),
                        KeyId = GetKeyIdOrThrow()
                    });

                    return decryptedData.Plaintext.ReadAllBytes();
                }
            
        }
    }
}