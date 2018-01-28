using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Olive.Mvc;

namespace Olive.ApiProxy
{
    public class MethodGenerator
    {
        MethodInfo Method;

        public MethodGenerator(MethodInfo method)
        {
            Method = method;
        }

        public string Generate()
        {
            var r = new StringBuilder();

            r.AppendLine($"public {MethodReturnType()} {Method.Name}({GetArgs()})");
            r.AppendLine("{");

            r.AppendLine($"var routeTemplate = new RouteTemplate(\"{Route()}\");");
            r.AppendLine("var url = routeTemplate.Merge(new { /* TODO: Inject the params */ });");

            r.AppendLine();
            r.AppendLine($"var client = Microservice.Api(\"{ProxyDLLGenerator.PublisherService}\", url);");
            r.AppendLine("Config?.Invoke(client);");

            r.AppendLine();
            r.Append($"return client.{HttpVerb()}");
            if (HttpVerb() == "Get") r.Append($"<{ReturnType()?.Name ?? "object"}>");

            r.Append("(new { /* TODO: Inject the params */ }");
            if (HttpVerb() == "Get") r.Append(", cacheChoice");
            r.AppendLine(");");

            r.AppendLine("}");

            return r.ToString();
        }

        string GetArgs()
        {
            var items = Method.GetParameters().Select(x => x.ParameterType.Name + " " + x.Name).ToList();

            if (HttpVerb() == "Get")
                items.Add("ApiResponseCache cacheChoice = ApiResponseCache.Accept");

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
            var routeAttr = Method.GetCustomAttributesData()
                .FirstOrDefault(x => x.AttributeType.Name == "Route" + "Attribute");

            if (routeAttr == null) return "{Route?}";

            return routeAttr.ConstructorArguments.Single().Value.ToString();
        }

        string MethodReturnType()
        {
            if (ReturnType() == null) return "Task";
            return "Task<" + ReturnType()?.Name + ">";
        }

        public Type ReturnType()
        {
            return (Method.GetCustomAttribute(typeof(ReturnsAttribute)) as ReturnsAttribute)?.ReturnType;
        }

        public Type[] GetCustomTypes()
        {
            var types = Method.GetParameters().Select(x => x.ParameterType).ToList();

            if (ReturnType() != null)
                types.Add(ReturnType());

            types = types.Distinct().ToList();

            types = types.Select(x => x.IsArray ? x.GetElementType() : x).Distinct().ToList();

            return types
                .Where(x => !x.FullName.StartsWith("System."))
                .Where(x => !x.FullName.StartsWith("Olive."))
                .ToArray();
        }
    }
}