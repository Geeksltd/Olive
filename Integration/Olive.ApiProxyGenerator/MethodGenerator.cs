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

        string ReturnType => Method.GetApiMethodReturnType()?.Name;

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

        string GetArg()
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

        string GetArgs()
        {
            var items = Method.GetParameters().Select(x => x.ParameterType.GetProgrammingName(useGlobal: false, useNamespace: false, useNamespaceForParams: false, useCSharpAlias: true) + " " + x.Name).ToList();

            return string.Join(", ", items);
        }

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