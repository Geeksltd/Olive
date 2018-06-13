using Microsoft.AspNetCore.DataProtection;

namespace Olive.Security
{
    public class SymmetricKeyDataProtector : IDataProtector
    {
        string Purpose;
        string EncryptionKey;

        public SymmetricKeyDataProtector(string encryptionKey)
        {
            EncryptionKey = encryptionKey;
        }

        public SymmetricKeyDataProtector(string purpose, string encryptionKey)
        {
            EncryptionKey = encryptionKey;
            Purpose = purpose;
        }

        public IDataProtector CreateProtector(string purpose)
            => new SymmetricKeyDataProtector(EncryptionKey, purpose);

        public byte[] Protect(byte[] plaintext)
            => Encryption.Encrypt(plaintext, EncryptionKey + Purpose).GZip();

        public byte[] Unprotect(byte[] protectedData)
            => Encryption.Decrypt(protectedData.UnGZip(), EncryptionKey + Purpose);
    }
}