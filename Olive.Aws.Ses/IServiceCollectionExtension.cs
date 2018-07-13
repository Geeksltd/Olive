using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Olive.Email;

namespace Olive.Aws.Ses
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddAwsSesProvider(this IServiceCollection @this)
        {
            return @this.AddTransient<IEmailDispatcher, AwsSesEmailDispatcher>();
        }
    }
}
