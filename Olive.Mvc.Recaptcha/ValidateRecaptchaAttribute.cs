using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Olive.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ValidateRecaptchaAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        /// <inheritdoc />
        public bool IsReusable => true;

        /// <inheritdoc />
        public int Order { get; set; }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService<IValidateRecaptchaFilter>() as ValidateRecaptchaFilter;
    }
}
