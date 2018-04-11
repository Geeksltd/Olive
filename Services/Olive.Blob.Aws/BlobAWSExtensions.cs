using Microsoft.Extensions.DependencyInjection;
using Olive.Entities;

namespace Olive
{
    public static class BlobAWSExtensions
    {
        public static IServiceCollection AddS3BlobStorageProvider(this IServiceCollection services)
        {
            return services.AddSingleton(typeof(IBlobStorageProvider), new BlobAws.S3BlobStorageProvider());
        }
    }
}