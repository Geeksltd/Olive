using System;
using System.Reflection;
using System.Text;

namespace Olive.ApiProxy
{
    internal class DtoProgrammer
    {
        Type Type;
        internal MethodInfo DatabaseGetMethod;

        public DtoProgrammer(Type type)
        {
            Type = type;
            DatabaseGetMethod = type.FindDatabaseGetMethod();
        }

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
            r.AppendLine("public class " + Type.Name + " : Olive.Entities.GuidEntity");
            r.AppendLine("{");

            if (DatabaseGetMethod != null)
            {
                r.AppendLine($"static {Type.Name}()");
                r.AppendLine("{");
                r.AppendLine("Olive.Entities.Data.Database.Instance");
                r.AppendLine($".RegisterDataProvider(typeof({Type.Name}), {Type.Name}DataProvider.Current);");
                r.AppendLine("}");
                r.AppendLine();
            }

            foreach (var p in Type.GetEffectiveProperties())
            {
                var type = p.GetPropertyOrFieldType().GetProgrammingName(useGlobal: false, useNamespace: false, useNamespaceForParams: false, useCSharpAlias: true);

                r.AppendLine($"/// <summary>Gets or sets the {p.Name} of this {Type.Name} instance.</summary>");
                r.AppendLine($"public {type} {p.Name} {{ get; set; }}");
                r.AppendLine();
            }

            r.AppendLine("}");
            return r.ToString();
        }

        internal string GenerateDataProvider()
        {
            var r = new StringBuilder();
            r.AppendLine("namespace " + Type.Namespace);
            r.AppendLine("{");
            r.AppendLine("using Olive;");
            r.AppendLine("using System;");
            r.AppendLine("using System.Threading.Tasks;");
            r.AppendLine("using Olive.Entities;");
            r.AppendLine();

            var type = Type.Name + "DataProvider";

            r.AppendLine($"/// <summary>Enables querying {Type.Name} instances through the Olive Database Api.</summary>");
            r.AppendLine($"public class {type} : LimitedDataProvider");
            r.AppendLine("{");
            r.AppendLine($"static Action<{Context.ControllerType.Name}> Configurator;");
            r.AppendLine();

            r.AppendLine($"/// <summary>Allows you to configure the Api Client used to fetch objects from the remote Api. You can use to, for instance, specify your caching choice.</summary>");
            r.AppendLine($"public static void Configure(Action<{Context.ControllerType.Name}> configurator) => Configurator = configurator;");
            r.AppendLine();

            r.AppendLine($"/// <summary>Gets a singleton instance of this data provider.</summary>");
            r.AppendLine($"public static {type} Current {{ get; }} = new {type}();");
            r.AppendLine();

            r.AppendLine($"/// <summary>Implements the standard Get() method of Database by delegating the call to the remote Web Api.</summary>");
            r.AppendLine("public override async Task<IEntity> Get(object id)");
            r.AppendLine("{");
            r.AppendLine($"var api = new {Context.ControllerType.Name}();");
            r.AppendLine("Configurator?.Invoke(api);");
            r.AppendLine($"return await api.{DatabaseGetMethod.Name}(id.ToString().To<Guid>());");
            r.AppendLine("}");

            r.AppendLine("}");
            r.AppendLine("}");
            return new CSharpFormatter(r.ToString()).Format();
        }
    }
}