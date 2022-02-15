using Olive.Security.Cloud;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Olive.Security.Cloud.CustomEncryption
{
    public class DataKeyService : IDataKeyService
    {
        const int AES_IV_LENGTH = 16;
        Aes EncryptionClient = CreateClient();

        private static Aes CreateClient()
        {
            var result = Aes.Create();
            result.Key = (Environment.GetEnvironmentVariable("Encryption:MasterKey").OrNullIfEmpty() ?? throw new Exception("Could not find the master key.")).ToBytes(System.Text.Encoding.UTF8);
            result.IV = result.Key.ToArray().Take(AES_IV_LENGTH).ToArray();
            return result;
        }

        public async Task<Key> GenerateKey()
        {
            var encryptionKey = Guid.NewGuid().ToString().Remove("-");
            var encryptionKeyBytes = encryptionKey.ToBytes(System.Text.Encoding.UTF8);
            return new Key
            {
                EncryptionKeyReference = Encrypt(encryptionKey),
                EncryptionKey = encryptionKeyBytes
            };
        }

        byte[] Encrypt(string message)
        {
            byte[] result;

            var encryptor = EncryptionClient.CreateEncryptor(EncryptionClient.Key, EncryptionClient.IV);
            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(message); // Write all data to the stream.
                }
                result = ms.ToArray();
            }

            return result;
        }


        public byte[] GetEncryptionKey(byte[] encryptionKeyReference)
        {
            var decryptor = EncryptionClient.CreateDecryptor(EncryptionClient.Key, EncryptionClient.IV);

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
            {
                cs.Write(encryptionKeyReference, 0, encryptionKeyReference.Length);
                cs.FlushFinalBlock();
                var result = ms.ReadAllBytes();
                return result;
            }
        }


    }
}