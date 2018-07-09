using Olive.Mvc;
using System;
using System.Linq;
using System.Reflection;

namespace Olive.ApiProxy
{
    static class Extensions
    {
        public static CustomAttributeData GetAttribute(this MemberInfo type, string attributeName)
        {
            return type.GetCustomAttributesData()
               .FirstOrDefault(x => x.AttributeType.Name == attributeName + "Attribute");
        }

        public static string GetExplicitAuthorizeServiceAttribute(this MemberInfo type)
        {
            if (type.GetAttribute("Authorize") != null)
                return null; // User specific, or multiple roles.

            foreach (var att in new[] { "AuthorizeTrustedService", "AuthorizeService" })
                if (type.GetAttribute(att) != null) return att;

            return null;
        }

        public static MemberInfo[] GetEffectiveProperties(this Type type)
        {
            return type.GetPropertiesAndFields(BindingFlags.Public | BindingFlags.Instance)
                 .Except(x => x.Name == "ID" && x.GetPropertyOrFieldType() == typeof(Guid))
                 .ToArray();
        }

        public static MethodInfo FindDatabaseGetMethod(this Type type)
        {
            return Context.ActionMethods.Select(x => x.Method).FirstOrDefault(x => x.GetCustomAttribute<RemoteDataProviderAttribute>()?.EntityType == type);
        }

        public static Type GetApiMethodReturnType(this MethodInfo @this)
        {
            return @this.GetAttribute("Returns")?.ConstructorArguments.Single().Value as Type;
        }
    }
}