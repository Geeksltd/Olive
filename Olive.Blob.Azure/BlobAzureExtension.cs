using Microsoft.Extensions.DependencyInjection;
using Olive.BlobAzure;
using Olive.Entities;
using System;

namespace Olive
{
    public static class BlobAzureExtension
    {
        public static IServiceCollection AddAzureBlobStorageProvider(this IServiceCollection @this)
        {
            return @this
                .AddSingleton<IBlobStorageProvider, AzureBlobStorageProvider>()
                .AddTransient<IAzureSasUrlGenerator, AzureSasUrlGenerator>();
        }

        public static IServiceCollection AddAzureBlobStorageProvider(this IServiceCollection @this, TimeSpan sasUrlTimeout)
        {
            return @this
                .AddSingleton<IBlobStorageProvider, AzureBlobStorageProvider>()
                .AddTransient<IAzureSasUrlGenerator>(_ => new AzureSasUrlGenerator(sasUrlTimeout));
        }
    }
}
