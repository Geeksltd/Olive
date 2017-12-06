using System.Threading.Tasks;
using Olive.Entities;

namespace Olive.Services.BlobAws
{
    public class S3BlobStorageProvider : IBlobStorageProvider
    {
        public Task Save(Blob document) => S3Proxy.Upload(document);

        public Task Delete(Blob document) => S3Proxy.Delete(document);

        public Task<byte[]> Load(Blob document) => S3Proxy.Load(document);

        public Task<bool> FileExists(Blob document) => S3Proxy.FileExists(document);

        public bool CostsToCheckExistence() => true;
    }
}