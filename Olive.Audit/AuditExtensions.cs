using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Olive
{
    public static class AuditExtensions
    {
        public static IServiceCollection AddDefaultAudit(this IServiceCollection @this)
        {
            @this.TryAddSingleton<Audit.IAudit, Audit.DefaultAudit>();

            return @this;
        }
    }
}
