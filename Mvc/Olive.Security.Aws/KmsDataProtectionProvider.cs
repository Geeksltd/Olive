using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Olive.Security.Aws.Services;

namespace Olive.Security.Aws
{
    public class KmsDataProtectionProvider : IDataProtector
    {
        static ConcurrentDictionary<string, byte[]> CachedDecrptedData = new ConcurrentDictionary<string, byte[]>();
        string Purpose;

        public IDataProtector CreateProtector(string purpose)
            => new KmsDataProtectionProvider { Purpose = purpose };

        public byte[] Protect(byte[] plaintext)
        {
            var key = Task.Factory.RunSync(() => DataKeyService.GenerateKey());

            var encryptedData = CreateProtector(key.Plain).Protect(plaintext);

            // To make it secure, we should combine the key's length, the key and the cipher data. 
            var cipher = key.Cipher;
            if (cipher.Length > byte.MaxValue)
                throw new Exception("Cipher key is longer than a byte!");

            return new byte[] { (byte)cipher.Length }.Concat(cipher, encryptedData).ToArray();
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            var cacheKey = protectedData.ToBase64String();
            return CachedDecrptedData.GetOrAdd(cacheKey, (key) =>
            {
                var keyLength = protectedData.First();
                var keyCipher = protectedData.Skip(1).Take(keyLength).ToArray();
                var dataPart = protectedData.Skip(1 + keyLength).ToArray();

                var encryptionKey = Task.Factory.RunSync(() => DataKeyService.GetPlainKey(keyCipher));

                return CreateProtector(encryptionKey).Unprotect(dataPart);
            });
        }

        SymmetricKeyDataProtector CreateProtector(byte[] encryptionKey)
            => new SymmetricKeyDataProtector(Purpose, encryptionKey.ToBase64String());
    }
}