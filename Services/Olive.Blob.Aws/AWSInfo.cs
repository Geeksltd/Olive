using Amazon;

namespace Olive.BlobAws
{
    /// <summary>This class is to help the AWS Bucket</summary>
    static class AWSInfo
    {
        internal static string S3BucketName => Config.Get("Blob:S3:Bucket");

        /// <summary>
        /// Returns the Amazaon Region Endpoint as it might change in future
        /// </summary>
        internal static RegionEndpoint S3Region
        {
            get
            {
                var regionName = Config.TryGet<string>("Blob:S3:Region");

                if (regionName.HasValue())
                    return RegionEndpoint.GetBySystemName(regionName);
                else
                    return RegionEndpoint.EUWest1;
            }
        }
    }
}