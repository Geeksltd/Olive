namespace Olive.MFA
{
    using Microsoft.Extensions.DependencyInjection;
    using Olive.SMS;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class MFAExtensions
    {
        public static IServiceCollection AddMFA(this IServiceCollection @this)
        {
            return @this
                .AddSingleton<IMFAManager, MFAManager>()
                 .AddSingleton<ITemporaryLoginService, TemporaryLoginService>();
        }
    }
}
