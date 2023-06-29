namespace Olive.Migration.Services.Contracts
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMigrationListService
    {
        Task<(List<IMigrationTask> tasks, List<string> errorMessages)> Get();
    }
}