using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace Olive.Mvc
{
    /// <summary>
    /// It will bind this property value from a query parameter in the following order: Form, Route, Querystring.
    /// The first provided value in the sources specified above will be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FromRequestAttribute : Attribute
    {
        public string Name { get; }
        public FromRequestAttribute(string name) => Name = name;

        internal IValueProvider CreateValueProvider(ModelBindingContext bindingContext) => new FromRequestValueProvider(Name, bindingContext);

        class FromRequestValueProvider : IValueProvider
        {
            public string Name { get; }

            ModelBindingContext BindingContext;

            public FromRequestValueProvider(string name, ModelBindingContext bindingContext)
            {
                Name = name;
                BindingContext = bindingContext;
            }

            public bool ContainsPrefix(string prefix) => ModelStateDictionary.StartsWithPrefix(prefix, Name);

            public ValueProviderResult GetValue(string key)
            {
                var request = BindingContext.HttpContext.Request;
                var actionContext = BindingContext.ActionContext;

                if (request.HasFormContentType && request.Form.ContainsKey(Name))
                    return new ValueProviderResult(request.Form[Name]);

                else if (actionContext.RouteData.Values.ContainsKey(Name))
                    return new ValueProviderResult(new StringValues(actionContext.RouteData.Values[Name].ToString()));

                else if (request.Query.ContainsKey(Name))
                    return new ValueProviderResult(request.Query[Name]);

                return new ValueProviderResult();
            }
        }
    }
}