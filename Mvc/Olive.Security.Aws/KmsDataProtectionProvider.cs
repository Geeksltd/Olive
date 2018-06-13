using Microsoft.AspNetCore.DataProtection;

namespace Olive.Security.Aws
{
    public class KmsDataProtectionProvider : IDataProtector
    {
        string Purpose;

        public IDataProtector CreateProtector(string purpose)
            => new KmsDataProtectionProvider { Purpose = purpose };

        public byte[] Protect(byte[] plaintext)
        {
            var encryptionKey = "Generate from AWS...";
            return new SymmetricKeyDataProtector(encryptionKey, Purpose).Protect(plaintext);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            var encryptionKey = "Find from AWS...";
            return new SymmetricKeyDataProtector(encryptionKey, Purpose).Unprotect(protectedData);
        }
    }
}