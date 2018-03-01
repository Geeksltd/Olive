using System;
using System.Text;

namespace Olive.ApiProxy
{
    class ProxyClassProgrammer
    {
        static Type Controller => Context.ControllerType;
        static string ClassName => Controller.Name;

        static bool ServiceOnly() => Controller.GetExplicitAuthorizeServiceAttribute().HasValue();

        public static string Generate()
        {
            var r = new StringBuilder();

            r.AppendLine("namespace " + Controller.Namespace);
            r.AppendLine("{");
            r.AppendLine("using System;");
            r.AppendLine("using System.Threading.Tasks;");
            r.AppendLine("using System.Collections.Generic;");
            r.AppendLine("using Olive;");
            r.AppendLine();
            r.Append("/// <summary>Provides access to the " + ClassName + " api of the " + Context.PublisherService + " service.");
            if (ServiceOnly())
                r.Append($" As the target Api declares [{Controller.GetExplicitAuthorizeServiceAttribute()}], my constructor will call AsServiceUser() automatically.");
            r.AppendLine("</summary>");
            r.AppendLine($"public class {ClassName} : StronglyTypedApiProxy");
            r.AppendLine("{");
            r.AppendLine();

            if (ServiceOnly())
                r.AppendLine($"public {ClassName}() => this.AsServiceUser();");

            foreach (var method in Context.ActionMethods)
            {
                r.AppendLine(method.Generate().Trim());
                r.AppendLine();
            }

            r.AppendLine("}");
            r.AppendLine("}");

            return new CSharpFormatter(r.ToString()).Format();
        }
    }
}
