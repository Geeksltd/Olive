using Microsoft.AspNetCore.DataProtection;

namespace Olive.Security.Aws
{
    public class KmsDataProtectionProvider : Cloud.DataProtectionProvider<DataKeyService>
    {
        public override IDataProtector CreateProtector(string purpose)
            => new KmsDataProtectionProvider { Purpose = purpose };
    }
}