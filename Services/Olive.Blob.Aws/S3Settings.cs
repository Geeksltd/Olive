namespace Olive.BlobAws
{
    class Settings
    {
        public static Amazon.RegionEndpoint RegionEndPoint
        {
            get
            {
                var specified = Config.Get("Aws:S3:Region");
                if (specified.HasValue()) return Amazon.RegionEndpoint.GetBySystemName(specified);
                else return Aws.RuntimeIdentity.Region;
            }
        }
    }
}