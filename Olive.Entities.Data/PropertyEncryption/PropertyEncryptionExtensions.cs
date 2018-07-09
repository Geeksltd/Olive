namespace Olive.Entities.Data
{
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;

    public static class PropertyEncryptionExtensions
    {
        public static void AddPropertyEncryption(this IServiceCollection services, Assembly domainAssembly)
        {
            EncryptedEntityInterceptor.Initialize(domainAssembly);
        }
    }
}