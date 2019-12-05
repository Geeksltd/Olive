using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

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
