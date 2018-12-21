using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Net;

namespace Olive.BlobAws
{
    public class S3BlobFileInfo : IFileInfo
    {
        readonly IAmazonS3 AmazonS3;
        readonly string BucketName;
        readonly string Key;

        GetObjectResponse FileObject;
        bool? Exist;

        public S3BlobFileInfo(string key)
        {
            AmazonS3 = AWSInfo.AmazonS3Client;
            BucketName = AWSInfo.S3BucketName;
            Key = key;
        }

        GetObjectResponse GetfileObject()
        {
            if (FileObject == null)
                FileObject = AsyncHelper.RunSync(() => AmazonS3.GetObjectAsync(BucketName, Key));

            return FileObject;
        }

        public string MD5 => GetfileObject().Metadata["Content-MD5"];

        public bool Exists
        {
            get
            {
                if (!Exist.HasValue)
                {
                    try
                    {
                        GetfileObject();
                        Exist = true;
                    }
                    catch (AmazonS3Exception e)
                    {
                        if (e.StatusCode == HttpStatusCode.NotFound) Exist = false;
                        throw;
                    }
                }

                return Exist.Value;
            }
        }

        public long Length => GetfileObject().ContentLength;

        /// <summary>
        /// A http url to the file, including the file name.
        /// </summary>
        public string PhysicalPath => $"s3-{AmazonS3.Config.RegionEndpoint.SystemName}.amazonaws.com/{GetfileObject().BucketName}/{GetfileObject().Key}";

        public string Name => Path.GetFileName(GetfileObject().Key.TrimEnd('/'));

        public DateTimeOffset LastModified => GetfileObject().LastModified;

        public bool IsDirectory => GetfileObject().Key.EndsWith("/");

        public Stream CreateReadStream() => GetfileObject().ResponseStream;
    }
}