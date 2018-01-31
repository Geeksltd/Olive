using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Olive.ApiProxy
{
    internal class DtoProgrammer
    {
        static List<Type> TypesToDefine;

        private Type Type;

        public DtoProgrammer(Type type) => Type = type;

        internal string Generate()
        {
            var r = new StringBuilder();

            r.AppendLine("namespace " + Type.Namespace);
            r.AppendLine("{");
            r.AppendLine("using System;");
            r.AppendLine("using System.Collections.Generic;");
            r.AppendLine();
            r.AppendLine("public class " + Type.Name);
            r.AppendLine("{");

            foreach (var p in Type.GetPropertiesAndFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var type = p.GetPropertyOrFieldType().GetProgrammingName(useGlobal: false, useNamespace: false, useNamespaceForParams: false, useCSharpAlias: true);
                r.AppendLine($"public {type} {p.Name} {{ get; set; }}");
            }

            r.AppendLine("}");
            r.AppendLine("}");

            return r.ToString();
        }

        static Type GetDefinableType(Type type)
        {
            if (type.IsArray) return GetDefinableType(type.GetElementType());
            if (type.Assembly != Context.Assembly) return null;
            return type;
        }

        internal static void CreateDtoTypes()
        {
            TypesToDefine = Context.ActionMethods.SelectMany(x => x.GetArgAndReturnTypes()).Distinct()
                .Select(x => GetDefinableType(x)).ExceptNull().Distinct().ToList();

            while (TypesToDefine.Any(t => Crawl(t))) continue;

            foreach (var type in TypesToDefine)
            {
                Console.Write("Adding DTO class " + type.Name + "...");
                File.WriteAllText(Context.Output + @"\" + type.Name + ".cs", new DtoProgrammer(type).Generate());
                Console.WriteLine("Done");
            }
        }

        static bool Crawl(Type type)
        {
            foreach (var member in type.GetPropertiesAndFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var memberType = GetDefinableType(member.GetPropertyOrFieldType());
                if (memberType == null || TypesToDefine.Contains(memberType)) continue;
                TypesToDefine.Add(memberType);
                return true;
            }

            return false;
        }
    }
}