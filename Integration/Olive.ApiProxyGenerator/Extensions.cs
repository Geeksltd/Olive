using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Olive.Mvc;

namespace Olive.ApiProxy
{
    static class Extensions
    {
        public static MethodInfo[] Validate(this MethodInfo[] methods)
        {
            var duplicateDataProviders = from method in methods
                                         let returnType = method.GetAttribute("Returns")?.ConstructorArguments.Single().Value as Type
                                         let markedAsRemoteDataProvider = method.Defines<RemoteDataProviderAttribute>()
                                         where returnType != null
                                         where markedAsRemoteDataProvider
                                         group method by returnType into g
                                         where g.HasMany()
                                         select g;

            if (duplicateDataProviders.Any())
            {
                var duplicateErrorMessage = (from g in duplicateDataProviders
                                             let type = g.Key
                                             let duplicateMenthodNames = g.ToList().Select(s => s.Name).ToString(",")
                                             select $"[{type} => {duplicateMenthodNames}]").ToString(",");

                throw new Exception($"Only one method can be marked as [RemoteDataProvider]. There are duplicates in {duplicateErrorMessage}");
            }

            return methods;
        }

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
            return Context.ActionMethods.FirstOrDefault(x => x.ReturnType() == type && x.IsGetDataprovider())?.Method;
        }

        public static bool IsIEnumerableOf(this Type type, Type T) => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericArguments().Contains(T) && i.GetInterfaces().Contains(typeof(IEnumerable)));
    }
}