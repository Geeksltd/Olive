using Amazon.S3.Model;
using Amazon.S3;
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
new S3PresignedUrlGenerator(PresignedUrlTimeout));
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

        public static string GetS3PresignedUrl(this Blob document, Action<GetPreSignedUrlRequest> config = null)
        {
            using (var client = new AmazonS3Client())
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = AWSInfo.S3BucketName,
                    Key = document.GetKey(),
                    Expires = AWSInfo.PreSignedUrlLifespan
                };

                request.ResponseHeaderOverrides.ContentType = document.GetMimeType();
                request.ResponseHeaderOverrides.ContentDisposition = $"attachment; filename=\"{document.FileName.Remove("\"", ",")}\"";

                config?.Invoke(request);

                return client.GetPreSignedURL(request);
            }
        }
    }
}