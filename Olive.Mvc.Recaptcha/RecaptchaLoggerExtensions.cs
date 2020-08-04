using Microsoft.Extensions.Logging;
using System;

namespace Olive.Mvc
{
    public static class RecaptchaLoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception> _validationException;

        static RecaptchaLoggerExtensions()
        {
            _validationException = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Looks up a localized string similar to Recaptcha validation failed. {Message}");
        }

        public static void ValidationException(this ILogger<ValidateRecaptchaFilter> logger, string message, Exception ex)
        {
            _validationException(logger, message, ex);
        }
    }
}
