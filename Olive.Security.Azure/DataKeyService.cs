using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Extensions.Configuration;
using Olive;
using Olive.Security.Cloud;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using azure = global::Azure;

namespace Olive.Security.Azure
{
    public class DataKeyService : IDataKeyService
    {
        readonly static ConcurrentDictionary<string, byte[]> EncryptionKeys = new ConcurrentDictionary<string, byte[]>();
        static string keyValutUri;
        static string masterKeyName;

        string KeyValutUri =>
          (keyValutUri ??= Context.Current.Config.GetValue("Azure:KeyVault:CookieAuthentication:Uri", Environment.GetEnvironmentVariable("AZURE_KEY_VALUT_COOKIE_AUTHENTICATION_URI"))) ?? throw new Exception("Azure Key Valut Authentication uri is not specified.");

        static string MasterKeyName =>
    (masterKeyName ??= Context.Current.Config.GetValue("Azure:KeyVault:CookieAuthentication:KeyName", Environment.GetEnvironmentVariable("AZURE_KEY_VALUT_COOKIE_AUTHENTICATION_KEY_NAME"))) ?? throw new Exception("Azure Key Valut Key Name is not specified.");

        KeyClient CreateClient()
          => new KeyClient(new Uri(KeyValutUri), new azure.Identity.DefaultAzureCredential());

        KeyWrapAlgorithm EncryptionAlgorithm => KeyWrapAlgorithm.Rsa15;

        async Task<CryptographyClient> GetCryptographyClient()
        {
            var client = CreateClient();

            var response = await client.GetKeyAsync(MasterKeyName);

            if (response.Value == null)
                throw new Exception($"Could not find a key with named {MasterKeyName}");

            return new CryptographyClient(response.Value.Id, new azure.Identity.DefaultAzureCredential());
        }

        public async Task<Key> GenerateKey()
        {
            var encryptionKey = Guid.NewGuid().ToString();
            var encryptionKeyBytes = encryptionKey.ToBytes(System.Text.Encoding.UTF8);
            var cryptoClient = await GetCryptographyClient();

            var encryptedKeyBytes = await cryptoClient.WrapKeyAsync(EncryptionAlgorithm, encryptionKeyBytes);

            return new Key
            {
                EncryptionKeyReference = encryptedKeyBytes.EncryptedKey,
                EncryptionKey = encryptionKeyBytes
            };
        }

        public byte[] GetEncryptionKey(byte[] encryptionKeyReference)
        {
            var keyRef = encryptionKeyReference.ToBase64String();

            return EncryptionKeys.GetOrAdd(keyRef,
                x => Task.Factory.RunSync(() => DownloadEncryptionKey(encryptionKeyReference)));
        }

        async Task<byte[]> DownloadEncryptionKey(byte[] encryptionKeyReference)
        {
            var cryptoClient = await GetCryptographyClient();

            var response = await cryptoClient.UnwrapKeyAsync(EncryptionAlgorithm, encryptionKeyReference);

            return response.Key;
        }
    }
}
