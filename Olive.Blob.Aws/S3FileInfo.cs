using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Net;

namespace Olive.BlobAws
{
    public class S3FileInfo : IFileInfo
    {
        readonly IAmazonS3 amazonS3;
        readonly string bucketName;
        readonly string key;

        GetObjectResponse fileObject;
        bool? exists;

        public S3FileInfo(IAmazonS3 amazonS3, string bucketName, string key)
        {
            this.amazonS3 = amazonS3;
            this.bucketName = bucketName;
            this.key = key;
        }

        GetObjectResponse getfileObject()
        {
            if (fileObject == null)
                fileObject = AsyncHelper.RunSync(() => amazonS3.GetObjectAsync(bucketName, key));

            return fileObject;
        }

        public string MD5 => getfileObject().Metadata["Content-MD5"];

        public bool Exists
        {
            get
            {
                if (!exists.HasValue)
                {
                    try
                    {
                        getfileObject();
                        exists = true;
                    }
                    catch (AmazonS3Exception e)
                    {
                        if (e.StatusCode == HttpStatusCode.NotFound) exists = false;
                        throw;
                    }
                }

                return exists.Value;
            }
        }

        public long Length => getfileObject().ContentLength;

        /// <summary>
        /// A http url to the file, including the file name.
        /// </summary>
        public string PhysicalPath => $"s3-{amazonS3.Config.RegionEndpoint.SystemName}.amazonaws.com/{getfileObject().BucketName}/{getfileObject().Key}";

        public string Name => Path.GetFileName(getfileObject().Key.TrimEnd('/'));

        public DateTimeOffset LastModified => getfileObject().LastModified;

        public bool IsDirectory => getfileObject().Key.EndsWith("/");

        public Stream CreateReadStream() => getfileObject().ResponseStream;
    }
}