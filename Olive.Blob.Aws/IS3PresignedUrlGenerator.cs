using Olive.Entities;
using System;

namespace Olive.BlobAws
{
    public interface IS3PresignedUrlGenerator
    {
        string Sign(Blob blob, TimeSpan? timeout = null);
        string Sign(string key, TimeSpan? timeout = null);
    }
}