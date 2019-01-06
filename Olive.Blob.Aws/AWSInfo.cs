using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Configuration;

namespace Olive.BlobAws
{
    /// <summary>This class is to help the AWS Bucket</summary>
    static class AWSInfo
    {
        static IConfiguration Config => Context.Current.Config;

        internal static string S3BucketName => Config.GetValue<string>("Blob:S3:Bucket");

        /// <summary>
        /// Returns the Amazaon Region Endpoint as it might change in future
        /// </summary>
        internal static RegionEndpoint S3Region
        {
            get
            {
                var regionName = Config.GetValue<string>("Blob:S3:Region");

                if (regionName.HasValue())
                    return RegionEndpoint.GetBySystemName(regionName);
                else
                    return RegionEndpoint.EUWest1;
            }
        }
    }
}