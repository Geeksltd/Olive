using Azure.Identity;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Security.Azure
{
    public static class Extensions
    {
        public static void ProtectKeysWithAzureKeyVault(this IDataProtectionBuilder builder, Uri keyVaultUri = null)
        {
            builder.ProtectKeysWithAzureKeyVault(keyVaultUri ?? DataKeyService.KeyValutUri, new DefaultAzureCredential());
        }
    }
}
