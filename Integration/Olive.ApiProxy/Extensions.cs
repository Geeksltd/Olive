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
    }
}