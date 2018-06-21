namespace Olive.BlobAws
{
    class Settings
    {
        public static Amazon.RegionEndpoint RegionEndPoint
            => Amazon.RegionEndpoint.GetBySystemName(Config.Get("Aws:S3:Region"));
    }
}