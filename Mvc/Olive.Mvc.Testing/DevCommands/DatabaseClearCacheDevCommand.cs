using Olive.Entities;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class DatabaseClearCacheDevCommand : IDevCommand
    {
        IDatabase Database;

        public DatabaseClearCacheDevCommand(IDatabase database) => Database = database;

        public string Name => "db-clear-cache";

        public string Title => "Clear DB cache";

        public bool IsEnabled() => true;

        public async Task<string> Run()
        {
            await Database.Refresh();
            return null;
        }
    }
}
