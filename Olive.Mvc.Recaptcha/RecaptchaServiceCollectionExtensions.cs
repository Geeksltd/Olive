using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace Olive.Mvc
{
    public static class RecaptchaServiceCollectionExtensions
    {
        public static void AddRecaptcha(this IServiceCollection services, RecaptchaOptions configureOptions = null)
        {
            services.TryAddSingleton(Options.Create(configureOptions));
            services.TryAddSingleton<IRecaptchaService, RecaptchaService>();
            services.TryAddSingleton<IRecaptchaValidationService>((sp) => sp.GetRequiredService<IRecaptchaService>());
            services.TryAddSingleton<IRecaptchaConfigurationService>((sp) => sp.GetRequiredService<IRecaptchaService>());
            services.TryAddSingleton<IValidateRecaptchaFilter, ValidateRecaptchaFilter>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static void AddRecaptcha(this IServiceCollection services, Action<RecaptchaOptions> configuration)
        {
            var configureOptions = new RecaptchaOptions();

            configuration(configureOptions);

            AddRecaptcha(services, configureOptions);
        }
    }
}
