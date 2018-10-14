using Microsoft.Extensions.DependencyInjection;
using System;

namespace Olive
{
    class BasicOliveServiceProvider : IServiceProvider
    {
        IServiceCollection Services;
        public BasicOliveServiceProvider(IServiceCollection services) => Services = services;

        public object GetService(Type serviceType)
        {
            var descriptor = Services.FirstOrDefault(x => x.ServiceType == serviceType);
            if (descriptor == null) return null;

            switch (descriptor.Lifetime)
            {
                case ServiceLifetime.Transient:
                    return descriptor.ImplementationFactory(this);
                case ServiceLifetime.Singleton:
                    return descriptor.ImplementationInstance;
                default:
                    throw new NotImplementedException($"{GetType().Name} does not support {descriptor.Lifetime} scope.");
            }
        }
    }
}
