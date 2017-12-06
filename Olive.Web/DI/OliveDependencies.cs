namespace Olive.Web
{
    public static class OliveDependencies
    {
        /// <summary>
        /// Inject the required dependencies.
        /// <para>It should be called in Startup.ConfigureServices</para>
        /// </summary>
        public static void Inject(IServiceCollection services)
        {
            services.AddSingleton(typeof(IHttpContextAccessor), typeof(HttpContextAccessor));
            services.AddSingleton(typeof(IActionContextAccessor), typeof(ActionContextAccessor));
        }

        /// <summary>
        /// Inject the required dependencies.
        /// <para>It should be called in Startup.ConfigureServices</para>
        /// </summary>
        public static void InjectOliveDependencies(this IServiceCollection services) => Inject(services);

        /// <summary>
        /// Configure the helper classes.
        /// <para>It should be called in Startup.Configure</para>
        /// </summary>
        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Context.Initialize(
                app,
                env,
                app.ApplicationServices.GetService<IHttpContextAccessor>(),
                app.ApplicationServices.GetService<IActionContextAccessor>()
                );
        }

        /// <summary>
        /// Configure the helper classes.
        /// <para>It should be called in Startup.Configure</para>
        /// </summary>
        public static void ConfigureOliveDependencies(this IApplicationBuilder app, IHostingEnvironment env) => Configure(app, env);
    }
}
