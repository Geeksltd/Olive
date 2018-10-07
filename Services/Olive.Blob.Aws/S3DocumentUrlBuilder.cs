﻿namespace Olive.BlobAws
{
    /// <summary>
    /// Builds the S3 Bucket url.
    /// </summary>
    class S3DocumentUrlBuilder
    {
        const string S3_BUCKET_URL_TEMPLATE = "https://s3-{0}.amazonaws.com/{1}/{2}";

        /// <summary>
        /// Gets the S3 Bucket Url using the specified bucket name and key.
        /// </summary>        
        internal static string GetUrl(string bucketName, string key)
        {
            return S3_BUCKET_URL_TEMPLATE.FormatWith(AWSInfo.S3Region.SystemName, bucketName, key);
        }
    }
}