using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Microsoft.Extensions.Configuration;
using Olive.Aws;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Olive.Security.Aws
{
    class DataKeyService
    {
        readonly static ConcurrentDictionary<string, byte[]> EncryptionKeys = new ConcurrentDictionary<string, byte[]>();

        static readonly string MasterKeyArn = Context.Current.Config.GetValue("Aws:Kms:MasterKeyArn",
            defaultValue: Environment.GetEnvironmentVariable("AWS_KMS_MASTERKEY_ARN"))
            .OrNullIfEmpty() ?? throw new Exception("Aws Master Key Arn is not specified.");

        static AmazonKeyManagementServiceClient CreateClient()
            => new AmazonKeyManagementServiceClient();

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
                    EncryptionKeyReference = dataKey.CiphertextBlob.ReadAllBytes(),
                    EncryptionKey = dataKey.Plaintext.ReadAllBytes()
                };
            }
        }

        internal static byte[] GetEncryptionKey(byte[] encryptionKeyReference)
        {
            var keyRef = encryptionKeyReference.ToBase64String();

            return EncryptionKeys.GetOrAdd(keyRef,
                x => Task.Factory.RunSync(() => DownloadEncryptionKey(encryptionKeyReference)));
        }

        static async Task<byte[]> DownloadEncryptionKey(byte[] encryptionKeyReference)
        {
            using (var kms = CreateClient())
            {
                var decryptedData = await kms.DecryptAsync(new DecryptRequest
                {
                    CiphertextBlob = encryptionKeyReference.AsStream()
                });

                return decryptedData.Plaintext.ReadAllBytes();
            }
        }

        internal class Key
        {
            public byte[] EncryptionKeyReference { get; set; }

            public byte[] EncryptionKey { get; set; }
        }
    }
}
