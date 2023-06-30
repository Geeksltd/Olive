namespace Olive.Migration.Services.Contracts
{
    using System.Threading.Tasks;

    public interface IBackupService
    {
        Task<(bool success, string path, string errorMessage)> Backup(string taskName, WhichBackup whichBackup);
    }
}