using Microsoft.Extensions.DependencyInjection;
using Olive.Entities;
using Microsoft.Extensions.Configuration;

namespace Olive
{
    public static class AWSExtensions
    {
        public static IServiceCollection UseAwsIdentity(this IServiceCollection @this)
        {
            Aws.RuntimeIdentity.Load();

            Config.AddConfiguration(x => x.AddConfiguration(new SecureConfiguration()));
            return @this;
        }
    }
}