using System.Threading.Tasks;

namespace Olive.Entities
{
    public interface IBlobStorageProvider
    {
        Task Save(Blob blob);
        Task Delete(Blob blob);
        Task<byte[]> Load(Blob blob);
        Task<bool> FileExists(Blob blob);
        bool CostsToCheckExistence();
    }
}

