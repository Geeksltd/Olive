using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Olive.ApiProxy
{
    internal class DtoProgrammer
    {
        Type Type;

        static string[] CommonDefaultProperties = new[] { "title", "name", "subject", "description", "label", "text" };

        MemberInfo[] EffectiveProperties => Type.GetEffectiveProperties();

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
            r.AppendLine();
            r.AppendLine(GenerateDto());

            r.AppendLine("}");

            return new CSharpFormatter(r.ToString()).Format();
        }

        string GenerateDto()
        {
            var r = new StringBuilder();
            r.AppendLine($"/// <summary>The {Type.Name} DTO type (Data Transfer Object) to be used with {Context.ControllerType.Name} Api.</summary>");
            r.AppendLine("[Olive.Entities.CacheObjects(false)]");
            r.AppendLine("public class " + Type.Name + " : Olive.Entities.GuidEntity");
            r.AppendLine("{");

            foreach (var p in EffectiveProperties)
            {
                var type = p.GetPropertyOrFieldType().GetProgrammingName(useGlobal: false, useNamespace: false, useNamespaceForParams: false, useCSharpAlias: true);

                r.AppendLine($"/// <summary>Gets or sets the {p.Name} of this {Type.Name} instance.</summary>");
                r.AppendLine($"public {type} {p.Name} {{ get; set; }}");
                r.AppendLine();
            }

            r.AppendLine(GenerateTheToString());

            r.AppendLine("}");
            return r.ToString();
        }

        string GenerateTheToString() => $"public override string ToString() => {GetToStringField()};";

        string GetToStringField()
        {
            var explicitToStringField = EffectiveProperties.SingleOrDefault(i => i.GetCustomAttribute<Entities.ToStringAttribute>() != null);
            var toStringField = explicitToStringField ?? SelectDefaultToStringField();

            if (toStringField != null) return toStringField.Name;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("********** WARNING ***********");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Could not find a suitable implementation for ToString().");
            Console.WriteLine("- There is no field annotated with [ToString] attribute.");
            Console.WriteLine($"- Nor is there a field in {Type.FullName} named {CommonDefaultProperties.Select(s => s.ToPascalCaseId()).ToString(", ", " or ")}.");
            Console.WriteLine();
            Console.WriteLine();
            Console.ResetColor();

            Console.WriteLine("This does not stop the proxy creation.");
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();

            return $"\"{Type.Name.TrimBefore("+", trimPhrase: true)}: No good ToString() :(\"";
        }

        MemberInfo SelectDefaultToStringField()
        {
            MemberInfo result;

            // Priority 1 = Accurate:
            result = CommonDefaultProperties.Select(n => EffectiveProperties.FirstOrDefault(p => p.Name.ToLower() == n)).FirstOrDefault(p => p != null);
            if (result != null) return result;

            // Priority 2 = Contains:
            result = CommonDefaultProperties.Select(n => EffectiveProperties.FirstOrDefault(p => p.Name.ToLower().Contains(n))).FirstOrDefault(p => p != null);
            if (result != null) return result;

            return null;
        }
    }
}