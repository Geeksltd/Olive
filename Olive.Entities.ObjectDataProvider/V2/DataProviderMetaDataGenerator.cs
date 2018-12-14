using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Olive.Entities.ObjectDataProvider.V2
{
    internal class DataProviderMetaDataGenerator
    {
        internal static DataProviderMetaData Generate(Type type)
        {
            var tableName = TableNameAttribute.GetTableName(type);

            return new DataProviderMetaData(type)
            {
                BaseClassTypesInOrder = GetParents(type),
                DrivedClassTypes = GetDrivedClasses(type),
                Properties = GetProperties(type).ToArray(),
                Schema = SchemaAttribute.GetSchema(type),
                TableName = tableName,
                TableAlias = tableName
            };
        }

        static IEnumerable<PropertyData> GetProperties(Type type)
        {
            var infos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var info in infos.Except(t => CalculatedAttribute.IsCalculated(t)))
            {
                yield return new PropertyData
                {
                    IsAutoNumber = AutoNumberAttribute.IsAutoNumber(info),
                    IsPrimaryKey = PrimaryKeyAttribute.IsPrimaryKey(info),
                    Name = info.Name,
                    PropertyInfo = info
                };
            }

            if(SoftDeleteAttribute.IsEnabled(type, inherit: false))
                yield return new PropertyData
                {
                    IsAutoNumber = false,
                    IsPrimaryKey = false,
                    Name = ".Deleted",
                    PropertyInfo = type.GetProperty("IsMarkedSoftDeleted"),
                    IsDeleted = true
                };

            yield return new PropertyData
            {
                IsAutoNumber = false,
                IsPrimaryKey = false,
                Name = "OriginalId",
                PropertyInfo = type.GetProperty("OriginalId"),
                IsOriginalId = true
            };
        }

        static Type[] GetDrivedClasses(Type type) =>
            type.Assembly.GetTypes().Where(t => t.IsA(type)).ToArray();

        static Type[] GetParents(Type type)
        {
            var result = new List<Type>();

            if (type.IsAnyOf(null,
                typeof(object),
                typeof(GuidEntity),
                typeof(IntEntity),
                typeof(StringEntity))) return result.ToArray();

            result.AddRange(GetParents(type.BaseType));
            result.Add(type);

            return result.ToArray();
        }
    }
}