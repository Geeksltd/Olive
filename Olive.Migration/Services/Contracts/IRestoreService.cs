namespace Olive.Migration.Services.Contracts
{
    using System.Threading.Tasks;

    public interface IRestoreService
    {
        Task<(bool success, string errorMessage)> Restore(string path);
    }
}