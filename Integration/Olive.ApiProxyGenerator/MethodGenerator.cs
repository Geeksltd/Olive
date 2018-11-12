using Olive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Olive.ApiProxy
{
    public class MethodGenerator
    {
        internal MethodInfo Method;
        string[] RouteParams;

        public string ReturnType => Method.GetApiMethodReturnType()?.Name;

        public MethodGenerator(MethodInfo method)
        {
            Method = method;
            RouteParams = new RouteTemplate(Route()).Parameters.Select(x => x.Key).ToArray();
        }

        public string Generate()
        {
            var r = new StringBuilder();

            r.Append($"/// <summary>Sends a Http{HttpVerb()} request to {Route()} of the {Context.PublisherService} service.");

            if (Method.GetExplicitAuthorizeServiceAttribute().HasValue())
                r.Append($" As the target Api declares [{Method.GetExplicitAuthorizeServiceAttribute()}], I will call AsServiceUser() automatically.");
            r.AppendLine("</summary>");

            r.AppendLine($"public Task{ReturnType.WithWrappers("<", ">")} {Method.Name}({GetArgs()})");
            r.AppendLine("{");
            //Inject the mock data here
            r.AppendLine("if(MockConfig.Enabled)");
            r.AppendLine("{");
            r.AppendLine($"return MockConfig.Expect.{Method.Name}Result({GetArguments().Keys.ToString(", ")});");
            r.AppendLine("}");
            if (Method.GetExplicitAuthorizeServiceAttribute().HasValue())
                r.AppendLine("this.AsServiceUser();");

            if (RouteParams.Any())
            {
                r.AppendLine($"var routeTemplate = new RouteTemplate(\"{Route()}\");");
                r.AppendLine($"var url = routeTemplate.Merge({RouteParams.ToString(", ").WithWrappers("new { ", "}")});");
            }
            else r.AppendLine($"var url = \"{Route()}\";");

            r.AppendLine();
            r.AppendLine($"var client = Microservice.Of(\"{Context.PublisherService}\").Api(url);");
            r.AppendLine("foreach (var config in Configurators) config(client);");
            r.AppendLine();
            r.AppendLine(GetReturnStatement());
            r.AppendLine("}");

            return r.ToString();
        }

        string GetReturnStatement()
        {
            var r = new StringBuilder();
            r.Append($"return client.{HttpVerb()}");
            if (HttpVerb() == "Get") r.Append($"<{ReturnType.Or("object")}>");
            r.AppendLine("(" + GetArg() + ");");
            return r.ToString();
        }

        public string GetArg()
        {
            var args = new List<string>();

            var parameters = Method.GetParameters().Select(x => x.Name).Except(RouteParams).Trim();

            if (parameters.None()) return null;

            if (HttpVerb() == "GET")
                return "new { " + parameters.ToString(", ") + "}";

            if (parameters.HasMany())
                throw new Exception(HttpVerb() + "-based Api methods can take only one argument.");

            return parameters.Single();
        }

        public string GetArgs() => GetArguments().Select(x => x.Value + " " + x.Key).ToString(", ");

        public string GetMockKeyExpression()
        {
            return GetArguments().Select(x => x.Value + ":{" + x.Key + "}").ToString(" | ");
        }

        /// <summary>
        /// Keys are arg names. Values are their types.
        /// </summary>
        Dictionary<string, string> GetArguments()
        {
            return Method.GetParameters().ToDictionary(x => x.Name,
                  x => x.ParameterType.GetProgrammingName(useGlobal: false, useNamespace: false, useNamespaceForParams: false, useCSharpAlias: true));
        }
        public string GetArgsNames()
        {
            var items = Method.GetParameters().Select(x => x.Name).ToList();
            return string.Join(", ", items);
        }

        public string GetArgsTypes() => GetArguments().Values.ToString(", ");

        string HttpVerb()
        {
            foreach (var item in new[] { "Get", "Post", "Put", "Patch", "Delete" })
                if (Method.CustomAttributes.Any(x => x.AttributeType.Name == "Http" + item + "Attribute"))
                    return item;

            return null;
        }

        string Route()
        {
            var classRouteAttr = Context.ControllerType
                      .GetAttribute("Route")
                      ?.ConstructorArguments
                      .FirstOrDefault().Value.ToString();

            return classRouteAttr.WithSuffix("/") + GetMethodRoute();
        }

        string GetMethodRoute()
        {
            foreach (var name in new[] { "Route", "HttpGet", "HttpPost", "HttpDelete", "HttpPut", "HttpPatch" })
            {
                var result = Method.GetAttribute(name)
                             ?.ConstructorArguments
                             .FirstOrDefault().Value.ToStringOrEmpty();

                if (result.HasValue()) return result;
            }

            return "{Route???}";
        }

        public Type[] GetArgAndReturnTypes()
        {
            return Method.GetParameters().Select(x => x.ParameterType)
                .Concat(Method.GetApiMethodReturnType()).ToArray();
        }
    }
}