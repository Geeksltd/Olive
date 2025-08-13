using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Olive
{
    public partial class Context
    {
        static Context current = new();
        internal static Func<Context> ContextProvider = () => new Context();

        Context.ScopedServiceProvider ScopeServiceProvider;

        /// <summary>
        /// Occurs when the StartUp.OnInitializedAsync is completed.
        /// </summary>
        public static event AwaitableEventHandler StartedUp;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Task OnStartedUp() => StartedUp.Raise();

        public static Context Current => current ?? throw new InvalidOperationException("Olive.Context is not initialized!");

        public static Context Initialize(IServiceProvider applicationServices, Func<IServiceProvider> scopeServiceProvider)
        {
            return current = new()
            {
                ScopeServiceProvider = new Context.ScopedServiceProvider(applicationServices, scopeServiceProvider)
            };
        }

        /// <summary>
        /// Gets a required service of the specified contract type.
        /// </summary>
        public TService GetService<TService>() => (TService)ScopeServiceProvider.GetRequiredService(typeof(TService));

        /// <summary>
        /// Gets a required service of the specified contract type.
        /// </summary>
        public object GetService(Type serviceType) => ScopeServiceProvider.GetRequiredService(serviceType);

        public IConfiguration Config => GetService<IConfiguration>();

        public TService GetOptionalService<TService>() where TService : class
        {
            var result = (TService)ScopeServiceProvider.GetService(typeof(TService));

            if (result == null)
                Debug.WriteLine(typeof(TService).FullName + " service is not configured.");

            return result;
        }

        public object GetOptionalService(Type serviceType)
        {
            var result = ScopeServiceProvider.GetService(serviceType);

            if (result == null)
                Debug.WriteLine(serviceType.FullName + " service is not configured.");

            return result;
        }

        public IEnumerable<TService> GetServices<TService>() => ScopeServiceProvider.GetServices(typeof(TService)).Cast<TService>();
        public IEnumerable<object> GetServices(Type serviceType) => ScopeServiceProvider.GetServices(serviceType);

        public sealed class ScopedServiceProvider
        {
            public IServiceProvider ApplicationServices { get; }
            public Func<IServiceProvider> ScopeServiceProvider { get; }

            public ScopedServiceProvider(IServiceProvider applicationServices, Func<IServiceProvider> scopeServiceProvider)
            {
                ApplicationServices = applicationServices ?? throw new ArgumentNullException(nameof(applicationServices));
                ScopeServiceProvider = scopeServiceProvider;
            }

            public object GetRequiredService(Type serviceType)
            {
                var provider = ScopeServiceProvider();
                if (provider != null)
                    return provider.GetRequiredService(serviceType);

                using var scope = ApplicationServices.CreateScope();
                var scopedProvider = scope.ServiceProvider;
                return scopedProvider.GetRequiredService(serviceType);
            }

            public object GetService(Type serviceType)
            {
                var provider = ScopeServiceProvider();
                if (provider != null)
                    return provider.GetService(serviceType);

                using var scope = ApplicationServices.CreateScope();
                var scopedProvider = scope.ServiceProvider;
                return scopedProvider.GetService(serviceType);
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                var provider = ScopeServiceProvider();
                if (provider != null)
                    return provider.GetServices(serviceType);

                using var scope = ApplicationServices.CreateScope();
                var scopedProvider = scope.ServiceProvider;
                return scopedProvider.GetServices(serviceType);
            }
        }
    }
}