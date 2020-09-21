using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.BlobAws
{
    public static class Extensions
    {
        public static string GetS3PresignedUrl(this Blob document) => S3Proxy.GetPresignedUrl(document);
    }
}
