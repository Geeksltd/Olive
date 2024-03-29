using Amazon.S3;
using Amazon.S3.Model;
using Olive.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Olive.BlobAws
{
    /// <summary>
    /// A helper class for common S3 actions.
    /// </summary>
    public class S3BlobStorageProvider : IBlobStorageProvider
    {
        const string FILE_NOT_FOUND = "NotFound";

        AmazonS3Client CreateClient => new AmazonS3Client();
        static ILogger Log => Olive.Log.For(typeof(S3BlobStorageProvider));

        public bool CostsToCheckExistence() => true;

        /// <summary>
        /// Uploads a document to the Amazon S3 Client.
        /// </summary>
        public async Task SaveAsync(Blob document)
        {
            using (var client = CreateClient)
            {
                try
                {
                    Log.Debug("Blob create upload request");
                    var request = await CreateUploadRequest(document);
                    Log.Debug("Blob create upload object");
                    var response = await client.PutObjectAsync(request);
                    Log.Debug("Blob response code: " + response.HttpStatusCode);

                    switch (response.HttpStatusCode)
                    {
                        case System.Net.HttpStatusCode.OK:
                        case System.Net.HttpStatusCode.Accepted: return;
                        default: throw new Exception($"AWS Upload for key {request.Key} returned: " + response.HttpStatusCode);
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug("Create request ex: " + ex.Message);
                    throw ex;
                }
            }
        }

        public async Task<bool> FileExistsAsync(Blob document)
        {
            try
            {
                using (var client = CreateClient)
                {
                    return await client.GetObjectMetadataAsync(AWSInfo.S3BucketName, document.GetKey()) != null;
                }
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode == FILE_NOT_FOUND) return false;

                throw;
            }
        }

        public async Task<byte[]> LoadAsync(Blob document)
        {
            var key = document.GetKey();

            try
            {
                return await Load(document.GetKey());
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to load s3 document with {key} key");
                throw;
            }
        }

        /// <summary>
        /// Deletes a document with the specified key name on the Amazon S3 server.
        /// </summary>
        public async Task DeleteAsync(Blob document)
        {
            var key = document.GetKey();

            using (var client = CreateClient)
            {
                var response = await client.DeleteObjectAsync(AWSInfo.S3BucketName, key);

                switch (response.HttpStatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                    case System.Net.HttpStatusCode.NoContent:
                    case System.Net.HttpStatusCode.Accepted: return;
                    default: throw new Exception("AWS DeleteObject for key " + key + " returned: " + response.HttpStatusCode);
                }
            }
        }

        async Task<byte[]> Load(string documentKey)
        {
            using (var client = CreateClient)
            {
                var request = CreateGetObjectRequest(documentKey);

                var responseStream = (await client.GetObjectAsync(request)).ResponseStream;

                using (var memoryStream = new MemoryStream())
                {
                    responseStream.CopyTo(memoryStream);

                    return await memoryStream.ReadAllBytesAsync();
                }
            }
        }

        GetObjectRequest CreateGetObjectRequest(string objectKey)
        {
            return new GetObjectRequest
            {
                BucketName = AWSInfo.S3BucketName,
                Key = objectKey
            };
        }

        async Task DeleteOlds(Blob document)
        {
            var oldVersionKeys = await GetOldVersionKeys(document);

            if (oldVersionKeys.Any())
                using (var client = CreateClient)
                {
                    var request = CreateDeleteOldsRequest(oldVersionKeys);
                    var response = await client.DeleteObjectsAsync(request);

                    switch (response.HttpStatusCode)
                    {
                        case System.Net.HttpStatusCode.OK:
                        case System.Net.HttpStatusCode.NoContent:
                        case System.Net.HttpStatusCode.Accepted: return;
                        default: throw new Exception("AWS DeleteObjects returned: " + response.HttpStatusCode);
                    }
                }
        }

        async Task<IEnumerable<KeyVersion>> GetOldVersionKeys(Blob document)
          => await GetOldKeys(document).Select(s => new KeyVersion { Key = s });

        DeleteObjectsRequest CreateDeleteOldsRequest(IEnumerable<KeyVersion> oldKeys)
        {
            return new DeleteObjectsRequest
            {
                BucketName = AWSInfo.S3BucketName,
                Objects = oldKeys.ToList()
            };
        }

        async Task<IEnumerable<string>> GetOldKeys(Blob document)
        {
            var key = document.GetKey();

            using (var client = CreateClient)
            {
                var request = CreateGetObjectsRequest(document);

                return (await client.ListObjectsAsync(request))
                    .S3Objects.Select(s => s.Key);
            }
        }

        ListObjectsRequest CreateGetObjectsRequest(Blob document)
        {
            var key = document.GetKey();
            var prefix = key.TrimEnd(document.FileExtension);

            return new ListObjectsRequest
            {
                BucketName = AWSInfo.S3BucketName,
                Prefix = prefix
            };
        }

        /// <summary>
        /// Creates an Amazon PutObjectRequest for the specified Document and key name.
        /// </summary>
        async Task<PutObjectRequest> CreateUploadRequest(Blob document)
        {
            return new PutObjectRequest
            {
                ContentType = document.GetMimeType(),
                BucketName = AWSInfo.S3BucketName,
                Key = document.GetKey(),
                InputStream = new MemoryStream(await document.GetFileDataAsync())
            };
        }
    }
}