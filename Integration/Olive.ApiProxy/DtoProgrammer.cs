using System;
using System.Reflection;
using System.Text;

namespace Olive.ApiProxy
{
    internal class DtoProgrammer
    {
        Type Type;

        public DtoProgrammer(Type type) => Type = type;

        internal string Generate()
        {
            var r = new StringBuilder();

            r.AppendLine("namespace " + Type.Namespace);
            r.AppendLine("{");
            r.AppendLine("using System;");
            r.AppendLine("using System.Collections.Generic;");
            r.AppendLine("using System.Threading.Tasks;");
            r.AppendLine("using System.Reflection;");
            r.AppendLine("using Olive.Entities;");
            r.AppendLine("using Olive.Entities.Data;");
            r.AppendLine();
            r.AppendLine("public class " + Type.Name + " : Olive.Entities.GuidEntity");
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
    }
}