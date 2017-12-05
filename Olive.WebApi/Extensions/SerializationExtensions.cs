namespace Olive.WebApi
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;

    static class SerializationExtensions
    {
        internal static bool MatchesRequestedType(this Type type, string requestedType)
        {
            if (!type.IsPublic) return false;
            if (type.Defines<JsonIgnoreAttribute>()) return false;

            var attributes = type.GetCustomAttributes<MatchTypeAttribute>();

            if (attributes.None()) return type.Name == requestedType;
            else return attributes.Any(x => x.Name == requestedType);
        }

        internal static bool MatchesRequestedProperty(this PropertyInfo property, string requestedPropertyName)
        {
            var attributes = property.GetCustomAttributes<JsonPropertyAttribute>().ToList();

            if (attributes.Any(a => a.PropertyName == requestedPropertyName)) return true;

            return attributes.None(x => x.PropertyName.HasValue()) && property.Name == requestedPropertyName;
        }
    }
}
