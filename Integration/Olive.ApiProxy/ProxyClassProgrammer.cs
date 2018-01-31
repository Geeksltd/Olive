using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.ApiProxy
{
    class ProxyClassProgrammer
    {
        static string ClassName => Context.ControllerType.Name;

        public static string Generate()
        {
            var r = new StringBuilder();

            r.AppendLine("namespace " + Context.ControllerType.Namespace);
            r.AppendLine("{");
            r.AppendLine("using System;");
            r.AppendLine("using System.Threading.Tasks;");
            r.AppendLine("using System.Collections.Generic;");
            r.AppendLine("using Olive;");
            r.AppendLine();
            r.AppendLine("/// <summary>Provides access to the " + ClassName + " api of the " + Context.PublisherService + " service.</summary>");
            r.AppendLine($"public class {ClassName} : StronglyTypedApiProxy");
            r.AppendLine("{");
            r.AppendLine();

            foreach (var method in Context.ActionMethods)
            {
                r.AppendLine(method.Generate().Trim());
                r.AppendLine();
            }

            r.AppendLine("}");
            r.AppendLine("}");

            return r.ToString();
        }
    }
}
