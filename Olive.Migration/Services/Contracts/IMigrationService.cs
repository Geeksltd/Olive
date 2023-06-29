namespace Olive.Migration.Services.Contracts
{
    using System.Threading.Tasks;

    public interface IMigrationService
    {
        Task<(IMigrationTask task, string errorMessage)> Migrate(IMigrationTask task);
    }
}