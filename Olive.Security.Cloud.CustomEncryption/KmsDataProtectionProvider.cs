using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using System;
using Olive;

namespace Olive.Security.Cloud.CustomEncryption
{
    public partial class CustomEncryptionDataProtectionProvider : DataProtectionProvider<DataKeyService>
    {
        public CustomEncryptionDataProtectionProvider()
        {

        }

        public override IDataProtector CreateProtector(string purpose)
            => new CustomEncryptionDataProtectionProvider() { Purpose = purpose };

    }
}