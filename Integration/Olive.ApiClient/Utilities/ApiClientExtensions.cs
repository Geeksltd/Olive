using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Olive
{
    class ClearApiCacheDevCommand : IDevCommand
    {
        public string Name => "api-clear-cache";

        public string Title => "Clear Api Cache";

        public bool IsEnabled() => true;

        public async Task<bool> Run()
        {
            await ApiClient.DisposeCache();
            return false;
        }
    }

    public static class ApiClientExtensions
    {
        public static DevCommandsOptions AddClearApiCache(
          this DevCommandsOptions @this)
        {
            @this.Services.AddSingleton<IDevCommand, ClearApiCacheDevCommand>();
            return @this;
        }
    }
}