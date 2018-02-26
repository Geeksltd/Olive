using Microsoft.AspNetCore.DataProtection;

namespace Olive.Security
{
    public class AsymmetricKeyDataProtector : IDataProtector
    {
        string Purpose;

        static string EncryptionKey => Config.GetOrThrow("Authentication:CookieEncryptionKey");
        static string DecryptionKey => Config.GetOrThrow("Authentication:CookieDecryptionKey");

        public AsymmetricKeyDataProtector(string purpose) => Purpose = purpose;

        public IDataProtector CreateProtector(string purpose) => new SymmetricKeyDataProtector(purpose);

        public byte[] Protect(byte[] plaintext)
            => Encryption.EncryptAsymmetric(plaintext, EncryptionKey + Purpose).GZip();

        public byte[] Unprotect(byte[] protectedData)
            => Encryption.DecryptAsymmetric(protectedData.UnGZip(), DecryptionKey + Purpose);
    }
}