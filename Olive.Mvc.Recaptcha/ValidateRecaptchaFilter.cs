using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Olive.Mvc
{
    public class ValidateRecaptchaFilter : IAsyncAuthorizationFilter, IValidateRecaptchaFilter
    {
        private readonly IRecaptchaValidationService Service;
        private readonly ILogger<ValidateRecaptchaFilter> Logger;
        private readonly IRecaptchaConfigurationService ConfigurationService;

        public ValidateRecaptchaFilter(IRecaptchaValidationService service, IRecaptchaConfigurationService configurationService, ILoggerFactory loggerFactory)
        {
            Service = service;
            ConfigurationService = configurationService;
            Logger = loggerFactory.CreateLogger<ValidateRecaptchaFilter>();
        }

        /// <inheritdoc />
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (ShouldValidate(context))
            {
                var formField = "g-recaptcha-response";
                void invalidResponse() => context.ModelState.AddModelError(formField, Service.ValidationMessage);

                try
                {
                    if (!context.HttpContext.Request.HasFormContentType)
                    {
                        throw new RecaptchaValidationException(
                            $"Looks up a localized string similar to The content type is '{context.HttpContext.Request.ContentType}', it should be form content.",
                            false);
                    }

                    var form = await context.HttpContext.Request.ReadFormAsync();
                    var response = form[formField];
                    var remoteIp = context.HttpContext.Connection?.RemoteIpAddress?.ToString();


                    if (string.IsNullOrEmpty(response))
                    {
                        invalidResponse();
                        return;
                    }

                    await Service.ValidateResponseAsync(response, remoteIp);
                }
                catch (RecaptchaValidationException ex)
                {
                    Logger.ValidationException(ex.Message, ex);

                    if (ex.InvalidResponse)
                    {
                        invalidResponse();
                        return;
                    }
                    else
                    {
                        context.Result = new BadRequestResult();
                    }
                }
            }
        }

        protected bool ShouldValidate(AuthorizationFilterContext context)
        {
            return ConfigurationService.Enabled && string.Equals("POST", context.HttpContext.Request.Method, StringComparison.OrdinalIgnoreCase);
        }
    }
}
