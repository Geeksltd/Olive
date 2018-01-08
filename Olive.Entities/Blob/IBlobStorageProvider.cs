using System.Threading.Tasks;

namespace Olive.Entities
{
    public interface IBlobStorageProvider
    {
        Task SaveAsync(Blob blob);
        Task DeleteAsync(Blob blob);
        Task<byte[]> LoadAsync(Blob blob);
        Task<bool> FileExistsAsync(Blob blob);
        bool CostsToCheckExistence();
    }
}

