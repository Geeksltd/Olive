using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Olive.Security.Cloud;


namespace Olive.Security.Aws
{
    public class KmsDataProtectionProvider : Cloud.DataProtectionProvider
    {
        public override IDataProtector CreateProtector(string purpose)
            => new KmsDataProtectionProvider { Purpose = purpose };

        protected override Task<Key> GenerateKey() => DataKeyService.GenerateKey();
        protected override byte[] GetDecryptionKey(byte[] encryptionKeyReference) => DataKeyService.GetEncryptionKey(encryptionKeyReference);
    }
}