using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Olive.Mvc;

namespace Olive.ApiProxy
{
    class DtoDataProviderClassGenerator
    {
        Type Type;
        internal MethodInfo ApiMethod;
        bool ApiMethodReturnsList => ApiMethod.GetApiMethodReturnType().IsIEnumerableOf(Type);

        public DtoDataProviderClassGenerator(Type type)
        {
            Type = type;
            ApiMethod = type.FindDatabaseGetMethod();
        }

        internal string Generate()
        {
            if (ApiMethod == null) return null;

            var r = new StringBuilder();
            r.AppendLine("namespace " + Type.Namespace);
            r.AppendLine("{");
            r.AppendLine("using Olive;");
            r.AppendLine("using System;");
            r.AppendLine("using System.Linq;");
            r.AppendLine("using System.Collections.Generic;");
            r.AppendLine("using System.Threading.Tasks;");
            r.AppendLine("using Olive.Entities;");
            r.AppendLine();

            var type = Type.Name + "DataProvider";

            r.AppendLine($"/// <summary>Enables querying {Type.Name} instances through the Olive Database Api.</summary>");
            r.AppendLine($"public class {type} : LimitedDataProvider");
            r.AppendLine("{");
            r.AppendLine($"static Action<{Context.ControllerType.Name}> Configurator;");
            if (ApiMethodReturnsList)
            {
                r.AppendLine($"static {Type.Name}[] LatestListResult;");
                r.AppendLine($"static Dictionary<Guid, {Type.Name}> LatestListResultByIds;");
            }
            r.AppendLine();
            r.AppendLine($"/// <summary>Gets a singleton instance of this data provider.</summary>");
            r.AppendLine($"public static {type} Current {{ get; }} = new {type}();");
            r.AppendLine();
            r.AppendLine(GenerateRegisterMethod());
            r.AppendLine();
            r.AppendLine(GenerateGetMethod());
            r.AppendLine("}");
            r.AppendLine("}");
            return new CSharpFormatter(r.ToString()).Format();
        }

        string GenerateRegisterMethod()
        {
            var r = new StringBuilder();
            r.AppendLine($"/// <summary>Allows you to configure the Api Client used to fetch objects from the remote Api. You can use to, for instance, specify your caching choice.</summary>");
            r.AppendLine($"public static void Register(Action<{Context.ControllerType.Name}> configurator = null)");
            r.AppendLine("{");
            r.AppendLine("Configurator = configurator;");
            r.AppendLine($"Context.Current.Database().RegisterDataProvider(typeof({Type.Name}), Current);");
            r.AppendLine("}");

            return r.ToString();
        }

        string GenerateGetMethod()
        {
            var r = new StringBuilder();
            r.AppendLine($"/// <summary>Implements the standard Get() method of Database by delegating the call to the remote Web Api.</summary>");
            r.AppendLine("public override async Task<IEntity> Get(object id)");
            r.AppendLine("{");
            r.AppendLine("if (id is null) return null;");

            if (ApiMethodReturnsList)
            {
                r.AppendLine("var guid = id.ToString().To<Guid>();");
                r.AppendLine("if (guid == Guid.Empty) return null;");
                r.AppendLine();
            }

            r.AppendLine($"var api = new {Context.ControllerType.Name}();");
            r.AppendLine("Configurator?.Invoke(api);");

            if (ApiMethodReturnsList)
            {
                r.AppendLine($"var listResult = await api.{ApiMethod.Name}();");
                r.AppendLine("if (!ReferenceEquals(listResult, LatestListResult))");
                r.AppendLine("{");
                r.AppendLine("LatestListResult = listResult;");
                r.AppendLine("LatestListResultByIds = listResult?.ToDictionary(x => x.ID, x => x);");
                r.AppendLine("}");
                r.AppendLine();
                r.AppendLine("return LatestListResultByIds?.GetOrDefault(guid);");
            }
            else
            {
                r.Append($"return await api.{ApiMethod.Name}(");

                var paramType = ApiMethod.GetParameters().Single().ParameterType;
                if (paramType == typeof(Guid)) r.Append("guid");
                else if (paramType == typeof(string)) r.Append("id.ToString()");
                else r.Append($"id.ToString().To<{paramType.Name}>()");
                r.AppendLine(");");
            }



            r.AppendLine("}");
            return r.ToString();
        }

        // MethodInfo FindDataProviderMethod()

        // var generateDefaultGet = Args().IsSingle() &&
        //                           Method.GetParameters().Single().ParameterType == typeof(Guid) &&
        //                           Method.GetApiMethodReturnType() == Method.ReturnType;

        // var methodName = HttpVerb();
        // var resultFilter = string.Empty;

        //    if (generateDefaultGet)
        //    {
        //        methodName = "Get";
        //    }
        //    else if (Method.GetApiMethodReturnType().IsIEnumerableOf(Method.ReturnType) && Args().None())
        //    {
        //        methodName = "All";
        //        resultFilter = ".FirstOrDefault()";
        //    }

        public static void ValidateRemoteDataProviderAttributes()
        {
            var relevantMethods = Context.ActionMethods.Select(x => x.Method)
                .Select(m => new
                {
                    Method = m,
                    ReturnsType = m.GetApiMethodReturnType(),
                    dataProviderType = m.GetCustomAttribute<RemoteDataProviderAttribute>()?.EntityType
                }).Where(x => x.dataProviderType != null).ToList();

            var moreThanOneDataProviderForAnyType = relevantMethods
               .GroupBy(x => x.dataProviderType)
               .FirstOrDefault(x => x.HasMany());

            if (moreThanOneDataProviderForAnyType != null)
            {
                throw new Exception($"Only one method can be marked as [RemoteDataProvider({moreThanOneDataProviderForAnyType.Key.Name})]:\n" + moreThanOneDataProviderForAnyType.Select(x => x.Method.DeclaringType.FullName + "." + x.Method.Name + "(...)").ToLinesString());
            }

            foreach (var method in relevantMethods)
            {
                if (method.ReturnsType == null)
                    throw new Exception($"Method {method.Method.Name} is marked as [RemoteDataProvider] but it does not specify [Returns] attribute.");

                if (method.ReturnsType.IsIEnumerableOf(method.dataProviderType)) continue;

                if (method.ReturnsType == method.dataProviderType)
                {
                    if (method.Method.GetParameters().None())
                        throw new Exception($"For {method.Method.Name} to be a valid [RemoteDataProvider({method.ReturnsType.Name})] it should take the object ID as parameter.");

                    if (method.Method.GetParameters().HasMany())
                        throw new Exception($"For {method.Method.Name} to be a valid [RemoteDataProvider({method.ReturnsType.Name})] it should take only one argument, which is the object ID.");

                    continue;
                }

                throw new Exception($"As method {method.Method.Name} is marked as [RemoteDataProvider({method.dataProviderType.Name})], its [Returns] type should be either {method.dataProviderType.Name} or {method.dataProviderType.Name}[].");
            }
        }
    }
}