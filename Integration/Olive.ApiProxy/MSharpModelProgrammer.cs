using System;
using System.Reflection;
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

            r.AppendLine("DatabaseMode(DatabaseOption.Transient).HasExternallyDomainClass();");

            foreach (var p in Type.GetPropertiesAndFields(BindingFlags.Public | BindingFlags.Instance))
                r.AppendLine(AddProperty(p.GetPropertyOrFieldType(), p.Name));

            r.AppendLine("}");
            r.AppendLine("}");
            r.AppendLine("}");

            return r.ToString();
        }

        string AddProperty(Type propertyType, string name)
        {
            var type = propertyType;
            if (type.IsArray) type = type.GetElementType();

            var method = type.Name;

            if (type.Assembly == Context.Assembly)
            {
                method = "Associate" + "ManyToMany".OnlyWhen(propertyType.IsArray) + "<" + type.Name + ">";
            }

            switch (method)
            {
                case "Boolean": method = "Bool"; break;
                default: break;
            }

            return method + "(\"" + name + "\");";
        }
    }
}