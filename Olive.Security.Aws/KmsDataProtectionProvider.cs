using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using System;
using Olive;

namespace Olive.Security.Aws
{
    public partial class KmsDataProtectionProvider : Cloud.DataProtectionProvider<DataKeyService>
    {
        public KmsDataProtectionProvider()
        {
            DataKeyService.MasterKeyArn = GetMasterKeyArn();
        }

        protected virtual string GetMasterKeyArn()
        {

            var fromEnvironment = Environment.GetEnvironmentVariable("AWS_KMS_MASTERKEY_ARN");

            return Context.Current.Config.GetValue("Aws:Kms:MasterKeyArn", defaultValue: fromEnvironment).OrNullIfEmpty()
                   ?? throw new Exception("Aws Master Key Arn is not specified.");
        }

        public override IDataProtector CreateProtector(string purpose)
            => new KmsDataProtectionProvider() { Purpose = purpose };

    }
}