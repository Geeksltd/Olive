using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Olive;
using Olive.Mvc;

namespace Olive.ApiProxy
{
    public class MethodGenerator
    {
        internal MethodInfo Method;
        string[] RouteParams;

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

            r.AppendLine($"public {MethodReturnType()} {Method.Name}({GetArgs()})");
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

        private string GetReturnStatement()
        {
            var result = new StringBuilder();

            var generateDefaultGet = Args().IsSingle() &&
                                     Method.GetParameters().Single().ParameterType == typeof(Guid) &&
                                     ReturnType() == Method.ReturnType;

            var methodName = HttpVerb();
            var resultFilter = string.Empty;

            if (generateDefaultGet)
            {
                methodName = "Get";
            }
            else if (ReturnType().IsIEnumerableOf(Method.ReturnType) && Args().None())
            {
                methodName = "All";
                resultFilter = ".FirstOrDefault()";
            }


            result.Append($"return client.{methodName}");
            if (methodName == "Get") result.Append($"<{ReturnType()?.Name ?? "object"}>");

            result.AppendLine("(" + Args().Trim().ToString(", ") + $"){resultFilter};");

            return result.ToString();
        }

        string[] Args()
        {
            var args = new List<string>();
            var queryString = Method.GetParameters().Select(x => x.Name)
                  .Except(RouteParams).Trim().ToString(", ").WithWrappers("new { ", " }");
            args.Add(queryString);

            return args.ToArray();
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
            var classRouteAttr = Context.ControllerType.GetAttribute("Route")
             ?.ConstructorArguments.Single().Value.ToString();

            var routeAttr = Method.GetAttribute("Route")
                ?.ConstructorArguments.Single().Value.ToString();

            return classRouteAttr.WithSuffix("/" + routeAttr.Or("{Route?}"));
        }

        string MethodReturnType()
        {
            if (ReturnType() == null) return "Task";
            return "Task<" + ReturnType()?.Name + ">";
        }

        public Type ReturnType()
        {
            return Method.GetAttribute("Returns")?.ConstructorArguments.Single().Value as Type;
        }

        public bool IsGetDataprovider()
        {
            if (ReturnType() == null || ReturnType().IsArray) return false;
            return Method.Defines<RemoteDataProviderAttribute>();
        }

        public Type[] GetArgAndReturnTypes()
        {
            return Method.GetParameters().Select(x => x.ParameterType).Concat(ReturnType()).ToArray();
        }
    }
}