using Olive.Entities;
using System;

namespace Olive.BlobAzure
{
    public interface IAzureSasUrlGenerator
    {
        string Sign(Blob blob, TimeSpan? timeout = null);
        string Sign(string key, TimeSpan? timeout = null);
    }
}
