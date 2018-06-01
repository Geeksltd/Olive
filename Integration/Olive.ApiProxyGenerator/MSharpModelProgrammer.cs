using System;
using System.Linq;
using System.Text;

namespace Olive.ApiProxy
{
    internal class MSharpModelProgrammer
    {
        Type Type;

        public MSharpModelProgrammer(Type type) => Type = type;

        internal string Generate()
        {
            var r = new StringBuilder();

            r.AppendLine("namespace " + Type.Namespace);
            r.AppendLine("{");
            r.AppendLine("using System;");
            r.AppendLine("using MSharp;");
            r.AppendLine();
            r.AppendLine("public class " + Type.Name + " : EntityType");
            r.AppendLine("{");
            r.AppendLine("public " + Type.Name + "()");
            r.AppendLine("{");

            r.AppendLine("HasExternallyDomainClass();");
            if (Type.FindDatabaseGetMethod() == null)
                r.AppendLine("DatabaseMode(DatabaseOption.Transient);");
            else r.AppendLine("DatabaseMode(DatabaseOption.Custom);");

            foreach (var p in Type.GetEffectiveProperties())
                r.AppendLine(AddProperty(p.GetPropertyOrFieldType(), p.Name));

            r.AppendLine("}");
            r.AppendLine("}");
            r.AppendLine("}");

            return new CSharpFormatter(r.ToString()).Format();
        }

        string AddProperty(Type propertyType, string name)
        {
            var type = propertyType;
            if (type.IsArray) type = type.GetElementType();

            bool isNullable;
            if (isNullable = type.IsNullable())
            {
                type = type.GetGenericArguments().Single();
            }

            var method = type.Name;

            if (type.Assembly == Context.Assembly)
            {
                method = "Associate" + "<" + type.Name + ">";
            }

            switch (method)
            {
                case "Boolean": method = "Bool"; break;
                case "Int32": method = "Int"; break;
                case "Int64": method = "Decimal"; break;
                default: break;
            }

            var result = method + "(\"" + name + "\")";

            if (type.Assembly == Context.Assembly && propertyType.IsArray)
                result += ".MaxCardinality(null)";

            if (!isNullable)
            {
                if (type.IsValueType)
                    result += ".Mandatory()";
            }

            return result + ";";
        }
    }
}