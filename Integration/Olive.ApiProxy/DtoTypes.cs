using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Olive.ApiProxy
{
    class DtoTypes
    {
        internal static List<Type> All;

        internal static void FindAll()
        {
            All = Context.ActionMethods.SelectMany(x => x.GetArgAndReturnTypes()).Distinct()
                .Select(x => GetDefinableType(x)).ExceptNull().Distinct().ToList();

            while (All.Any(t => Crawl(t))) continue;
        }

        internal static Type GetDefinableType(Type type)
        {
            if (type.IsArray) return GetDefinableType(type.GetElementType());
            if (type.Assembly != Context.Assembly) return null;
            return type;
        }

        static bool Crawl(Type type)
        {
            foreach (var member in type.GetEffectiveProperties())
            {
                var memberType = GetDefinableType(member.GetPropertyOrFieldType());
                if (memberType == null || All.Contains(memberType)) continue;
                All.Add(memberType);
                return true;
            }

            return false;
        }
    }
}
