using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Security.Cloud
{
    public abstract class DataProtectionProvider<TDataKeyService> : IDataProtector where TDataKeyService : IDataKeyService, new()
    {
        protected IDataKeyService DataKeyService;
        static ConcurrentDictionary<string, byte[]> CachedDecrptedData = new ConcurrentDictionary<string, byte[]>();
        protected string Purpose;

        public abstract IDataProtector CreateProtector(string purpose);

        public DataProtectionProvider() => DataKeyService = new TDataKeyService();

        protected Task<Key> GenerateKey() => DataKeyService.GenerateKey();

        protected byte[] GetDecryptionKey(byte[] encryptionKeyReference) => DataKeyService.GetEncryptionKey(encryptionKeyReference);

        public byte[] Protect(byte[] plaintext)
        {
            var key = Task.Factory.RunSync(() => GenerateKey());

            var encryptedData = CreateProtector(key.EncryptionKey).Protect(plaintext);

            // To make it secure, we should combine the key's length, the key and the cipher data. 
            var cipher = key.EncryptionKeyReference;
            var cipherBytes = BitConverter.GetBytes(cipher.Length);

            if (cipherBytes.Length > byte.MaxValue)
                throw new Exception("Cipher key length is longer than a byte!");

            return new byte[] { (byte)cipherBytes.Length }
            .Concat(cipherBytes)
            .Concat(cipher, encryptedData).ToArray().GZip();
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            protectedData = protectedData.UnGZip();

            var cacheKey = protectedData.ToBase64String();

            return CachedDecrptedData.GetOrAdd(cacheKey, (key) =>
            {
                var keyBufferLength = protectedData.First();
                var keyLength = BitConverter.ToInt32(protectedData.Skip(1).Take(keyBufferLength).ToArray());

                var encryptionKeyReference = protectedData.Skip(1 + keyBufferLength).Take(keyLength).ToArray();
                var dataPart = protectedData.Skip(1 + keyBufferLength + keyLength).ToArray();

                var encryptionKey = GetDecryptionKey(encryptionKeyReference);
                return CreateProtector(encryptionKey).Unprotect(dataPart);
            });
        }

        SymmetricKeyDataProtector CreateProtector(byte[] encryptionKey)
            => new SymmetricKeyDataProtector(Purpose, encryptionKey.ToBase64String());
    }
}