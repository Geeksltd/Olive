using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

        internal IValueProvider CreateValueProvider(ModelBindingContext bindingContext)
        {
            return new FromRequestValueProvider(Name, bindingContext);
        }

        class FromRequestValueProvider : IValueProvider
        {
            public string Name { get; }
            public FromRequestValueProvider(string name, ModelBindingContext bindingContext)
            {
                // TODO ...
            }
            public bool ContainsPrefix(string prefix)
            {
                return false; // ??
            }
            public ValueProviderResult GetValue(string key)
            {
                // TODO: Find the value in the order...
                return new ValueProviderResult();
            }
        }

    }
}