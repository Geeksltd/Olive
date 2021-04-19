using Microsoft.AspNetCore.DataProtection;
using Olive.Security.Cloud;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Security.Azure
{
    public class KeyVaultDataProtectionProvider : Olive.Security.Cloud.DataProtectionProvider<DataKeyService>
    {
        public override IDataProtector CreateProtector(string purpose)
            => new KeyVaultDataProtectionProvider { Purpose = purpose };
    }
}