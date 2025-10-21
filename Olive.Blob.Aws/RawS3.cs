using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.BlobAws
{
    public static class RawS3
    {
        public static string BucketName => Config.Get<string>("Blob:S3:Bucket");
        public static string Region => Config.Get<string>("Blob:S3:Region").Or(Config.Get<string>("Aws:Region"));


        public static Task<List<string>> GetFiles(string prefix = null, string extension = null) => GetFiles(BucketName, prefix, extension);
        public static async Task<List<string>> GetFiles(string bucket, string prefix = null, string extension = null)
        {
            var result = new List<string>();
            string continuationToken = null;

            var regionEndpoint = RegionEndpoint.GetBySystemName(Region);
            using var client = new AmazonS3Client(regionEndpoint);

            do
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = bucket,
                    Prefix = prefix?.TrimStart('/'), // S3 keys don't start with /
                    ContinuationToken = continuationToken
                };

                var response = await client.ListObjectsV2Async(request);

                foreach (var s3Object in response.S3Objects)
                {
                    if (extension.HasValue())
                    {
                        if (s3Object.Key.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                            result.Add(s3Object.Key);
                    }
                    else
                    {
                        result.Add(s3Object.Key);
                    }
                }

                continuationToken = response.IsTruncated == true ? response.NextContinuationToken : null;

            } while (continuationToken != null);

            return result;
        }


        public static Task UploadToS3(string name, Stream file, string contentType = null) => UploadToS3(BucketName, name, file, contentType);
        public static async Task UploadToS3(string bucket, string name, Stream file, string contentType = null)
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(Region);

            using var client = new AmazonS3Client(regionEndpoint);
            using var fileTransferUtility = new TransferUtility(client);

            await fileTransferUtility.UploadAsync(new TransferUtilityUploadRequest
            {
                InputStream = file,
                Key = name,
                BucketName = bucket,
                ContentType = contentType,
            });
        }


        public static Task Copy(string sourceKey, string destinationKey) => Copy(BucketName, sourceKey, destinationKey);
        public static Task Copy(string bucketName, string sourceKey, string destinationKey) => Copy(bucketName, bucketName, sourceKey, destinationKey);
        public static async Task Copy(string sourceBucketName, string destinationBucketName, string sourceKey, string destinationKey)
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(Region);
            using var client = new AmazonS3Client(regionEndpoint);
            await client.CopyObjectAsync(new CopyObjectRequest
            {
                SourceBucket = sourceBucketName,
                SourceKey = sourceKey,
                DestinationBucket = destinationBucketName,
                DestinationKey = destinationKey
            });
        }



        public static Task Move(string sourceKey, string destinationKey) => Move(BucketName, sourceKey, destinationKey);
        public static Task Move(string bucketName, string sourceKey, string destinationKey) => Move(bucketName, bucketName, sourceKey, destinationKey);
        public static async Task Move(string sourceBucketName, string destinationBucketName, string sourceKey, string destinationKey)
        {
            await Copy(sourceBucketName, destinationBucketName, sourceKey, destinationKey);
            await Remove(sourceBucketName, sourceKey);
        }


        public static Task Remove(string? objectPublicUrlOrKey) => Remove(BucketName, objectPublicUrlOrKey);
        public static async Task Remove(string bucket, string? objectPublicUrlOrKey)
        {
            if (objectPublicUrlOrKey.IsEmpty()) return;
            var name = objectPublicUrlOrKey.RemoveBeforeAndIncluding("amazonaws.com/");

            var regionEndpoint = RegionEndpoint.GetBySystemName(Region);

            using var client = new AmazonS3Client(regionEndpoint);

            var response = await client.DeleteObjectAsync(new DeleteObjectRequest
            {
                Key = name,
                BucketName = bucket
            });
        }


        public static Task Remove(List<string> objectPublicUrlsOrKeys) => Remove(BucketName, objectPublicUrlsOrKeys);
        public static async Task Remove(string bucket, List<string> objectPublicUrlsOrKeys)
        {
            var names = objectPublicUrlsOrKeys
                .Select(a => new KeyVersion { Key = a.RemoveBeforeAndIncluding("amazonaws.com/"), VersionId = null })
                .ToList();

            var regionEndpoint = RegionEndpoint.GetBySystemName(Region);

            using var client = new AmazonS3Client(regionEndpoint);

            await client.DeleteObjectsAsync(new DeleteObjectsRequest
            {
                Objects = names,
                BucketName = bucket
            });
        }


        public static Task<bool> Exists(string key) => Exists(BucketName, key);
        public static async Task<bool> Exists(string bucket, string key)
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(Region);

            using var client = new AmazonS3Client(regionEndpoint);

            try
            {
                var response = await client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                {
                    BucketName = bucket,
                    Key = key
                });

                return response != null;
            }

            catch (Exception ex)
            {
                return false;
            }
        }


        public static Task<string> ReadTextFile(string key) => ReadTextFile(BucketName, key);
        public static async Task<string> ReadTextFile(string bucket, string key)
        {
            using var reader = new StreamReader(await ReadFile(bucket, key));
            return await reader.ReadToEndAsync();
        }


        public static Task<T> ReadJsonFile<T>(string key) => ReadJsonFile<T>(BucketName, key);
        public static async Task<T> ReadJsonFile<T>(string bucket, string key)
        {
            var text = await ReadTextFile(bucket, key);
            return text.IsEmpty() ? default : JsonConvert.DeserializeObject<T>(text);
        }


        public static Task WriteTextFile(string key, string text) => WriteTextFile(BucketName, key, text);
        public static Task WriteTextFile(string bucket, string key, string text)
        {
            return WriteFile(bucket, key, x => x.ContentBody = text);
        }


        public static Task WriteJsonFile(string key, object data) => WriteJsonFile(BucketName, key, data);
        public static Task WriteJsonFile(string bucket, string key, object data)
        {
            return WriteFile(bucket, key, x => x.ContentBody = JsonConvert.SerializeObject(data));
        }


        public static Task<Stream> ReadFile(string key) => ReadFile(BucketName, key);
        public static async Task<Stream> ReadFile(string bucket, string key)
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(Region);
            using var client = new AmazonS3Client(regionEndpoint);
            var response = await client.GetObjectAsync(bucket, key);
            return response.ResponseStream;
        }



        public static Task WriteFile(string key, Action<PutObjectRequest> set) => WriteFile(BucketName, key, set);
        public static async Task WriteFile(string bucket, string key, Action<PutObjectRequest> set)
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(Region);
            using var client = new AmazonS3Client(regionEndpoint);

            var request = new PutObjectRequest { BucketName = bucket, Key = key };
            set(request);
            await client.PutObjectAsync(request);
        }


        public static Task<string> GeneratePreSignedURL(string key, TimeSpan duration) => GeneratePreSignedURL(BucketName, key, duration);
        public static async Task<string> GeneratePreSignedURL(string bucket, string key, TimeSpan duration)
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(Region);
            using var client = new AmazonS3Client(regionEndpoint);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = key,
                Expires = DateTime.UtcNow.Add(duration),
                Verb = HttpVerb.GET
            };

            string url = await client.GetPreSignedURLAsync(request);
            return url;
        }
    }
}