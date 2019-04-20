namespace Olive.Mvc
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Olive.Entities.Data;
    using System.Reflection;

    public static class PropertyEncryptionExtensions
    {
        public static void UsePropertyEncryption(this IApplicationBuilder @this, Assembly domainAssembly)
        {
            EncryptedEntityInterceptor.Initialize(domainAssembly);
        }

        /// <summary>
        /// Add default encrupted entitiy interceptors.
        /// </summary>
        public static void AddEntityInterceptor(this IServiceCollection @this)
        {
            @this.AddSingleton<IEntitySavingInterceptor, EncryptedEntitySavingInterceptor>();
            @this.AddSingleton<IEntityLoadedInterceptor, EncryptedEntityLoadedInterceptor>();
        }

        /// <summary>
        /// Add custom entitiy interceptors.
        /// </summary>
        public static void AddEntityInterceptor<SavingInterceptor, LoadedInterceptor>(this IServiceCollection @this)
           where SavingInterceptor : class, IEntitySavingInterceptor
           where LoadedInterceptor : class, IEntityLoadedInterceptor
        {
            @this.AddSingleton<IEntitySavingInterceptor, SavingInterceptor>();
            @this.AddSingleton<IEntityLoadedInterceptor, LoadedInterceptor>();
        }
    }
}