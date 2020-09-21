using Microsoft.Extensions.DependencyInjection;
using Olive.BlobAws;
using Olive.Entities;
using Olive.Mvc;
using System;

namespace Olive
{
    public static class BlobAWSExtensions
    {
        public static IServiceCollection AddS3BlobStorageProvider(
            this IServiceCollection @this,
            TimeSpan PresignedUrlTimeout)
        {
            return @this
                .AddSingleton<IBlobStorageProvider, S3BlobStorageProvider>()
                .AddTransient<IS3PresignedUrlGenerator, S3PresignedUrlGenerator>(serviceProvider =>
                {
                    return new S3PresignedUrlGenerator(PresignedUrlTimeout);
                });
        }

        public static IServiceCollection AddS3BlobStorageProvider(this IServiceCollection @this)
        {
            return @this
                .AddSingleton<IBlobStorageProvider, S3BlobStorageProvider>()
                .AddTransient<IS3PresignedUrlGenerator, S3PresignedUrlGenerator>();
        }

        public static IServiceCollection AddS3FileRequestService(this IServiceCollection @this)
        {
            return @this.AddTransient<FileUploadSettings>()
                .AddTransient<IFileUploadMarkupGenerator, S3FileUploadMarkupGenerator>()
                .AddTransient<IFileRequestService, S3FileRequestService>();
        }
    }
}