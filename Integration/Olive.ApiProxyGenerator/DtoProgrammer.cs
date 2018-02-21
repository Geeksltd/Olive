using System;
using System.Reflection;
using System.Text;

namespace Olive.ApiProxy
{
    internal class DtoProgrammer
    {
        Type Type;
        MethodInfo DatabaseGetMethod;

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

            if (DatabaseGetMethod != null)
                r.AppendLine(GenerateDataProvider());
            r.AppendLine("}");

            return r.ToString();
        }

        string GenerateDto()
        {
            var r = new StringBuilder();
            r.AppendLine("public class " + Type.Name + " : Olive.Entities.GuidEntity");
            r.AppendLine("{");

            if (DatabaseGetMethod != null)
            {
                r.AppendLine($"static {Type.Name}()");
                r.AppendLine("{");
                r.AppendLine("Olive.Entities.Data.Database.Instance");
                r.AppendLine($".RegisterDataProvider(typeof({Type.Name}), new {Type.Name}DataProvider());");
                r.AppendLine("}");
                r.AppendLine();
            }

            foreach (var p in Type.GetEffectiveProperties())
            {
                var type = p.GetPropertyOrFieldType().GetProgrammingName(useGlobal: false, useNamespace: false, useNamespaceForParams: false, useCSharpAlias: true);
                r.AppendLine($"public {type} {p.Name} {{ get; set; }}");
            }

            r.AppendLine("}");
            return r.ToString();
        }

        string GenerateDataProvider()
        {
            var r = new StringBuilder();
            r.AppendLine();
            r.AppendLine("public class " + Type.Name + "DataProvider : LimitedDataProvider");
            r.AppendLine("{");
            r.AppendLine("public override async Task<IEntity> Get(object id)");
            r.AppendLine($"=> await new {Context.ControllerType.Name}().{DatabaseGetMethod.Name}((Guid)id);");
            r.AppendLine("}");
            return r.ToString();
        }
    }
}