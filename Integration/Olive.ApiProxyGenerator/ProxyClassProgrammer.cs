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
            r.AppendLine($"public partial class {ClassName} : StronglyTypedApiProxy");
            r.AppendLine("{");
            r.AppendLine("static Action<ApiClient> DefaultConfiguration = x => x.Retries(3).CircuitBreaker();");
            r.AppendLine();
            r.AppendLine("public static TimeSpan? DefaultCacheExpiry { get; set; }");
            r.AppendLine();

            r.AppendLine("/// <summary>Creates a new instance of this Api Proxy with the default configuration..</summary>");
            r.AppendLine($"public {ClassName}() => this.Use(DefaultConfiguration){".AsServiceUser()".OnlyWhen(ServiceOnly())};");
            r.AppendLine();

            r.AppendLine("/// <summary>Sets the default configuration for instances of this Api proxy.</summary>");
            r.AppendLine("public static void DefaultConfig(Action<ApiClient> config)");
            r.AppendLine("=> DefaultConfiguration = config;");
            r.AppendLine();

            r.AppendLine("/// <summary>Creates an Api proxy that prefers cache for quickest result.</summary>");
            r.AppendLine($"public static {ClassName} Fast() => new {ClassName}().Cache(CachePolicy.CacheOrFreshOrFail, DefaultCacheExpiry);");
            r.AppendLine();

            r.AppendLine("/// <summary>Creates an Api proxy that prefers fresh cache policy for most up-to-date result.</summary>");
            r.AppendLine($"public static {ClassName} Fresh() => new {ClassName}().Cache(CachePolicy.FreshOrCacheOrFail, DefaultCacheExpiry);");

            r.AppendLine("/// <summary>Creates an Api proxy with the specified cache policy.</summary>");
            r.AppendLine($"public static {ClassName} As(CachePolicy policy) => new {ClassName}().Cache(policy, DefaultCacheExpiry);");
            r.AppendLine();

            foreach (var method in Context.ActionMethods)
            {
                r.AppendLine(method.Generate().Trim());
                r.AppendLine();
            }

            r.AppendLine("}");
            r.AppendLine("}");

            return new CSharpFormatter(r.ToString()).Format();
        }
        public static string GenerateMock()
        {
            var r = new StringBuilder();

            r.AppendLine("namespace " + Controller.Namespace);
            r.AppendLine("{");
            r.AppendLine("using System;");
            r.AppendLine("using System.Threading.Tasks;");
            r.AppendLine("using System.Collections.Generic;");
            r.AppendLine("using Olive;");
            r.AppendLine();
            r.Append("/// <summary>This will allow api users to set mock data for the api");
            r.AppendLine("</summary>");
            r.AppendLine($"public partial class {ClassName}");
            r.AppendLine("{");
            //Adding mock configuration field
            r.AppendLine($"static {ClassName}MockConfiguration {ClassName}MockConfiguration;");
            r.AppendLine();
            //Method Mock starts here
            r.Append($"/// <summary>set the mock configuration for {ClassName}");
            r.AppendLine("</summary>");
            r.AppendLine($"public static void Mock(Action<{ClassName}MockConfiguration> mockConfiguration, bool enabled = true)");
            r.AppendLine("{");
            r.AppendLine($"{ClassName}MockConfiguration.Enabled=enabled;");
            r.AppendLine($"mockConfiguration({ClassName}MockConfiguration);");
            //Method Mock ends here
            r.AppendLine("}");

            //class defination ends here
            r.AppendLine("}");

            //Namespace defination ends here
            r.AppendLine("}");

            return new CSharpFormatter(r.ToString()).Format();
        }
    }
}
