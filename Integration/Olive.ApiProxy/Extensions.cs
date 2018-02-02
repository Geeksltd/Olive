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
    }
}