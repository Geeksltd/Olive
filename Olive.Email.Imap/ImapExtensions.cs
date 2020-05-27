using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Olive.Email
{
    public static class ImapExtensions
    {
        public static IServiceCollection AddEmailFailureService<TEmailFailureService>(this IServiceCollection @this)
            where TEmailFailureService : class, IEmailFailureService
        {
            @this.TryAddTransient<IEmailFailureService, TEmailFailureService>();
            @this.TryAddTransient<IImapService, ImapService>();

            return @this;
        }

        public static IServiceCollection AddImapService(this IServiceCollection @this)
        {
            @this.TryAddTransient<IImapService, ImapService>();
            return @this;
        }
    }
}
