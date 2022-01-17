using Olive.Entities;
using System.Threading.Tasks;

namespace Olive.Mvc.Testing
{
    class DatabaseClearCacheDevCommand : IDevCommand
    {
        static IDatabase Database => Olive.Context.Current.Database();

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
