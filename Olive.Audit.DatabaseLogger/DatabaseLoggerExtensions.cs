using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Olive.Audit;

namespace Olive
{
    public static class DatabaseLoggerExtensions
    {
        public static IServiceCollection AddDatabaseLogger(this IServiceCollection @this)
        {
            return @this.AddTransient<Audit.IAuditLogger>(new DatabaseLogger());
        }
    }
}
