namespace Olive.Entities.Data
{
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;

    public static class PropertyEncryptionExtensions
    {
        public static void AddPropertyEncryption(this IServiceCollection @this, Assembly domainAssembly)
        {
            @this.AddSingleton<IEntitySavingInterceptor, EncryptedEntitySavingInterceptor>();
            @this.AddSingleton<IEntityLoadedInterceptor, EncryptedEntityLoadedInterceptor>();
            EncryptedEntityInterceptor.Initialize(domainAssembly);
        }
        public static void AddPropertyEncryption<SavingInterceptor, LoadedInterceptor>(this IServiceCollection @this, Assembly domainAssembly)
            where SavingInterceptor : class, IEntitySavingInterceptor
            where LoadedInterceptor : class, IEntityLoadedInterceptor
        {
            @this.AddSingleton<IEntitySavingInterceptor, SavingInterceptor>();
            @this.AddSingleton<IEntityLoadedInterceptor, LoadedInterceptor>();
            EncryptedEntityInterceptor.Initialize(domainAssembly);
        }
    }
}