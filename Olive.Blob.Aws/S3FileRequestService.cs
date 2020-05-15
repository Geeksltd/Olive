using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Olive.Entities;
using Olive.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.BlobAws
{
    class S3FileRequestService : IFileRequestService
    {
        private readonly string TempBucket;
        private readonly string Region;

        public S3FileRequestService(FileUploadSettings settings)
        {
            TempBucket = settings.BucketName;
            Region = settings.Region;
        }

        [EscapeGCop("It could be costfull to use those extensions.")]
        public async Task<Blob> Bind(string fileKey)
        {
            using (var client = GetClient())
            {
                var listRequest = new ListObjectsRequest
                {
                    BucketName = TempBucket,
                    Prefix = fileKey.WithSuffix("/"),
                };

                var response = await client.ListObjectsAsync(listRequest);

                if (response.S3Objects.Count == 0)
                    throw new Exception("There is no file in the temp folder " + fileKey);

                if (response.S3Objects.Count > 1)
                    throw new Exception("There are multiple files in the temp folder " + fileKey);

                var obj = response.S3Objects.Single();

                var objectRequest = new GetObjectRequest
                {
                    BucketName = TempBucket,
                    Key = obj.Key,
                };

                var file = await client.GetObjectAsync(objectRequest);
                var content = await file.ResponseStream.ReadAllBytesAsync();
                var filename = file.Key.RemoveBeforeAndIncluding(fileKey.WithSuffix("/"));

                return new Blob(content, filename);
            }
        }

        public async Task<object> CreateDownloadAction(byte[] data, string filename)
        {
            using (var client = GetClient())
            {
                var uniqueValue = Guid.NewGuid().ToString();
                var key = $"{uniqueValue}/{filename}";

                var request = new PutObjectRequest
                {
                    Key = key,
                    BucketName = TempBucket,
                    CannedACL = S3CannedACL.PublicRead,
                    InputStream = new MemoryStream(data),
                };

                await client.PutObjectAsync(request);

                return new
                {
                    Download = S3DocumentUrlBuilder.GetUrl(Region, TempBucket, key)
                };
            }
        }

        private AmazonS3Client GetClient() => new AmazonS3Client();

        public Task DeleteTempFiles(TimeSpan _)
        {
            throw new InvalidOperationException("S3 should take care of this.");
        }

        public Task<ActionResult> Download(string key)
        {
            throw new InvalidOperationException("Client should download from S3.");
        }

        public Task<object> TempSaveUploadedFile(IFormFile file, bool allowUnsafeExtension = false)
        {
            throw new InvalidOperationException("Client should upload to S3 directly.");
        }
    }

}
