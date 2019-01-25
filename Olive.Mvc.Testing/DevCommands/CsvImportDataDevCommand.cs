using Olive;
using Olive.Csv;
using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;

namespace Olive.Mvc.Testing
{
    class CsvImportDataDevCommand : DevCommand
    {
        const string PARAM_KEY = "path";

        public override string Name => "csv-import";

        IDatabase Database = Context.Current.Database();

        public override async Task<string> Run()
        {
            var path = Context.Current.Request().Param(PARAM_KEY);

            if (path.IsEmpty()) return "the path argument is not provided.";

            if (File.Exists(path))
                await ImportFile(path.AsFile());

            if (Directory.Exists(path))
                foreach (var file in path.AsDirectory().EnumerateFiles())
                    await ImportFile(file);

            return "Done";
        }

        async Task ImportFile(FileInfo file)
        {
            var dataTable = await CsvReader.ReadAsync(file, isFirstRowHeaders: true);
            var type = LoadType(file.NameWithoutExtension());

            var properties = GetProperties(dataTable, type).ToArray();

            if (properties.None() || properties.Any(p => p == null))
                throw new ArgumentException($"Failed to extract properties out of the columns' name.\r\nExtracted properties are {properties.ExceptNull().Select(p => p.Name).ToString(", ", " and ")}.");

            foreach (var row in dataTable.GetRows())
            {
                var instance = (IEntity) Activator.CreateInstance(type);

                for (int index = 0; index < properties.Length; index++)
                    properties[index].SetValue(instance, 
                        row[index].ToString().To(properties[index].PropertyType));

                await Database.Save(instance, SaveBehaviour.BypassAll);
            }
        }

        IEnumerable<PropertyInfo> GetProperties(DataTable dataTable, Type type)
        {
            foreach (var col in dataTable.Columns.Cast<DataColumn>())
                yield return type.GetProperty(col.ColumnName);
        }

        Type LoadType(string typeName)
        {
            return typeof(IEntity).FindImplementerClasses().FirstOrDefault(t => t.Name == typeName) ?? 
                throw new ArgumentException($"Could not load the type with the filename ({typeName}).");
        }
    }
}
