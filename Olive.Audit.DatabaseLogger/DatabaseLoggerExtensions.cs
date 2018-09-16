using Microsoft.Extensions.DependencyInjection;
using Olive.Audit;

namespace Olive
{
    public static class DatabaseLoggerExtensions
    {
        public static IServiceCollection AddDatabaseLogger(this IServiceCollection @this)
        {
            return @this.AddSingleton<Audit.IAuditLogger>(new DatabaseLogger());
        }
    }
}
