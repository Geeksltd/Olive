using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Olive.Security.Cloud;


namespace Olive.Security.Aws
{
    public class KmsDataProtectionProvider : Cloud.DataProtectionProvider<DataKeyService>
    {
        public override IDataProtector CreateProtector(string purpose)
            => new KmsDataProtectionProvider { Purpose = purpose };
    }
}