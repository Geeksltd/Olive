using Microsoft.Extensions.DependencyInjection;
using Olive.BlobAzure;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive
{
    public static class BlobAzureExtension
    {
        public static IServiceCollection AddAzureBlobStorageProvider(this IServiceCollection @this)
        {
            return @this
                .AddSingleton<IBlobStorageProvider, AzureBlobStorageProvider>();
        }
    }
}
