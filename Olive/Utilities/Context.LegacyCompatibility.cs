using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Olive
{
    partial class Context
    {
        /// <summary>
        /// Initializes Olive context for legacy (non .NET Core apps).
        /// </summary>
        public static void InitializeLegacy()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var services = new ServiceCollection();
            services.AddLogging();
            Initialize(services);
            Current.Configure(new BasicOliveServiceProvider(services));
        }

        class BasicOliveServiceProvider : IServiceProvider
        {
            IServiceCollection Services;
            public BasicOliveServiceProvider(IServiceCollection services) => Services = services;

            public object GetService(Type serviceType)
            {
                var result = Services.FirstOrDefault(x => x.ServiceType == serviceType).ImplementationType.CreateInstance();
                if (result != null) return result;
                else throw new Exception("Implementation not specified: " + serviceType.FullName);
            }
        }
    }
}