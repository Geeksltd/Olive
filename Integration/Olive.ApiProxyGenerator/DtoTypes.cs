﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Olive.ApiProxy
{
    class DtoTypes
    {
        internal static List<Type> All;

        internal static List<Type> Enums;

        internal static void FindAll()
        {
            All = Context.ActionMethods
                .SelectMany(x => x.GetArgAndReturnTypes())
                .ExceptNull()
                .Distinct()
                .Select(x => GetDefinableType(x))
                .ExceptNull()
                .Distinct()
                .ToList();

            Enums = All.Where(x => x.IsEnum).ToList();
            All = All.Except(Enums).ToList();

            while (All.Any(t => Crawl(t))) continue;
        }

        internal static Type GetDefinableType(Type type)
        {
            if (type.IsArray) return GetDefinableType(type.GetElementType());

            if (type.IsA<IEnumerable>() && type.GenericTypeArguments.IsSingle())
                return GetDefinableType(type.GenericTypeArguments.Single());

            if (type.Assembly != Context.Assembly) return null;
            return type;
        }

        static bool Crawl(Type type)
        {
            foreach (var member in type.GetEffectiveProperties())
            {
                var memberType = GetDefinableType(member.GetPropertyOrFieldType());
                if (memberType == null || All.Contains(memberType)) continue;

                if (memberType.IsEnum)
                {
                    if (Enums.Lacks(memberType)) Enums.Add(memberType);
                }
                else
                {
                    All.Add(memberType);
                    return true;
                }
            }

            return false;
        }
    }
}