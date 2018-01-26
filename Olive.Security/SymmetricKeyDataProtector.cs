using System;
using Microsoft.AspNetCore.DataProtection;

namespace Olive.Security
{
    public class SymmetricKeyDataProtector : IDataProtector
    {
        string Purpose;
        static string EncryptionKey;

        static SymmetricKeyDataProtector()
        {
            EncryptionKey = Config.GetOrThrow("Authentication:CookieDataProtectorKey");
        }

        public SymmetricKeyDataProtector(string purpose)
        {
            Purpose = purpose;
        }

        public IDataProtector CreateProtector(string purpose)
        {
            return new SymmetricKeyDataProtector(purpose);
        }

        public byte[] Protect(byte[] plaintext)
            => Encryption.Encrypt(plaintext, EncryptionKey + Purpose);

        public byte[] Unprotect(byte[] protectedData)
            => Encryption.Decrypt(protectedData, EncryptionKey + Purpose);
    }
}