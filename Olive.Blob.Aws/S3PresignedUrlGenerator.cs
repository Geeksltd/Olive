using Amazon.S3;
using Amazon.S3.Model;
using Olive.Entities;
using Olive;
using System;
using System.Collections.Generic;
using System.Text;
namespace Olive.BlobAws
{
    public class S3PresignedUrlGenerator : IS3PresignedUrlGenerator
    {
        public TimeSpan DefaultTimeout { get; }

        public S3PresignedUrlGenerator() : this(30.Minutes()) { }

        public S3PresignedUrlGenerator(TimeSpan defaultTimeout) => DefaultTimeout = defaultTimeout;

        public string Sign(Blob blob, TimeSpan? timeout = null) => Sign(blob.GetKey(), timeout);

        public string Sign(string key, TimeSpan? timeout = null)
        {
            using (var client = new AmazonS3Client())
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = AWSInfo.S3BucketName,
                    Key = key,
                    Expires = LocalTime.Now.Add(timeout ?? DefaultTimeout)
                };

                return client.GetPreSignedURL(request);
            }
        }
    }
}
