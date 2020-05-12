using Microsoft.Extensions.DependencyInjection;
using Olive.BlobAws;
using Olive.Entities;
using Olive.Mvc;

namespace Olive
{
    public static class BlobAWSExtensions
    {
        public static IServiceCollection AddS3BlobStorageProvider(this IServiceCollection @this)
        {
            return @this.AddSingleton(typeof(IBlobStorageProvider), new S3BlobStorageProvider());
        }

        public static IServiceCollection AddS3FileRequestService(this IServiceCollection @this)
        {
            return @this.AddTransient<IFileRequestService, S3FileRequestService>();
        }
    }
}