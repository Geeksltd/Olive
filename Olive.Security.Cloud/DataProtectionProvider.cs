﻿using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Security.Cloud
{
    public abstract class DataProtectionProvider : IDataProtector
    {
        static ConcurrentDictionary<string, byte[]> CachedDecrptedData = new ConcurrentDictionary<string, byte[]>();
        protected string Purpose;

        public abstract IDataProtector CreateProtector(string purpose);

        protected abstract Task<Key> GenerateKey();
        protected abstract byte[] GetDecryptionKey(byte[] encryptionKeyReference);

        public byte[] Protect(byte[] plaintext)
        {
            var key = Task.Factory.RunSync(() => GenerateKey());

            var encryptedData = CreateProtector(key.EncryptionKey).Protect(plaintext);

            // To make it secure, we should combine the key's length, the key and the cipher data. 
            var cipher = key.EncryptionKeyReference;
            if (cipher.Length > byte.MaxValue)
                throw new Exception("Cipher key is longer than a byte!");

            return new byte[] { (byte)cipher.Length }
            .Concat(cipher, encryptedData).ToArray().GZip();
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            protectedData = protectedData.UnGZip();

            var cacheKey = protectedData.ToBase64String();
            return CachedDecrptedData.GetOrAdd(cacheKey, (key) =>
            {
                var keyLength = protectedData.First();
                var encryptionKeyReference = protectedData.Skip(1).Take(keyLength).ToArray();
                var dataPart = protectedData.Skip(1 + keyLength).ToArray();

                var encryptionKey = GetDecryptionKey(encryptionKeyReference);
                return CreateProtector(encryptionKey).Unprotect(dataPart);
            });
        }

        SymmetricKeyDataProtector CreateProtector(byte[] encryptionKey)
            => new SymmetricKeyDataProtector(Purpose, encryptionKey.ToBase64String());
    }
}