using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.BlobAws
{
    public class S3Settings
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }

        public Amazon.RegionEndpoint RegionEndPoint => Amazon.RegionEndpoint.GetBySystemName(Region);

        public static S3Settings Current = Config.Bind<S3Settings>("Aws:S3");
    }
}
