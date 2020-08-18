using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Olive
{
    class BasicOliveServiceProvider : IServiceProvider
    {
        IServiceCollection Services;
        public BasicOliveServiceProvider(IServiceCollection services) => Services = services;

        static Dictionary<Type, object> Singleton = new Dictionary<Type, object>();

        object GetOrCreate(ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null) return descriptor.ImplementationInstance;
            if (descriptor.ImplementationFactory != null) return descriptor.ImplementationFactory(this);
            if (descriptor.ImplementationType != null) return descriptor.ImplementationType.CreateInstance();
            throw new Exception("No implementation specified!");
        }

        public object GetService(Type serviceType)
        {
            var descriptor = Services.FirstOrDefault(x => x.ServiceType == serviceType);
            if (descriptor == null) return null;

            switch (descriptor.Lifetime)
            {
                case ServiceLifetime.Transient:
                    return GetOrCreate(descriptor);
                case ServiceLifetime.Singleton:
                    if (Singleton.TryGetValue(serviceType, out var result)) return result;
                    return Singleton[serviceType] = GetOrCreate(descriptor);
                default:
                    throw new NotImplementedException($"{GetType().Name} does not support {descriptor.Lifetime} scope.");
            }
        }
    }
}
