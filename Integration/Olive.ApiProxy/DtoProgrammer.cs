using System;
using System.Text;

namespace Olive.ApiProxy
{
    internal class DtoProgrammer
    {
        private Type Type;

        public DtoProgrammer(Type type) => Type = type;

        internal string Generate()
        {
            var r = new StringBuilder();

            r.AppendLine("namespace " + Type.Namespace);
            r.AppendLine("{");
            r.AppendLine("public class " + Type.Name);
            r.AppendLine("{");
            foreach (var p in Type.GetProperties())
            {
                r.AppendLine("public " + p.PropertyType + " " + p.Name + " { get; set; }");
            }
            foreach (var p in Type.GetFields())
            {
                r.AppendLine("public " + p.FieldType + " " + p.Name + " { get; set; }");
            }
            r.AppendLine("}");
            r.AppendLine("}");

            return r.ToString();
        }
    }
}