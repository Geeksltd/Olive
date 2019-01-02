using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Olive.BlobAws
{
    public class S3FileInfo : IFileInfo
    {
        readonly IAmazonS3 AmazonS3;
        readonly string BucketName, Key;

        GetObjectResponse FileObject;
        bool? exists;

        public S3FileInfo(IAmazonS3 amazonS3, string bucketName, string key)
        {
            AmazonS3 = amazonS3;
            BucketName = bucketName;
            Key = key;
        }

        GetObjectResponse GetFileObject()
        {
            if (FileObject == null)
                FileObject = Task.Factory.RunSync(() => AmazonS3.GetObjectAsync(BucketName, Key));

            return FileObject;
        }

        public string MD5 => GetFileObject().Metadata["Content-MD5"];

        public bool Exists
        {
            get
            {
                if (exists.HasValue) return exists.Value;

                try
                {
                    GetFileObject();
                    exists = true;
                }
                catch (AmazonS3Exception e)
                {
                    if (e.StatusCode == HttpStatusCode.NotFound) exists = false;
                    else throw;
                }

                return exists.Value;
            }
        }

        public long Length => GetFileObject().ContentLength;

        public string Name => Path.GetFileName(GetFileObject().Key.TrimEnd('/'));

        /// <summary>
        /// A http url to the file, including the file name.
        /// </summary>
        public string PhysicalPath
            => $"s3-{AmazonS3.Config.RegionEndpoint.SystemName}.amazonaws.com/{GetFileObject().BucketName}/{GetFileObject().Key}";

        public DateTimeOffset LastModified => GetFileObject().LastModified;

        public bool IsDirectory => GetFileObject().Key.EndsWith("/");

        public Stream CreateReadStream() => GetFileObject().ResponseStream;
    }
}