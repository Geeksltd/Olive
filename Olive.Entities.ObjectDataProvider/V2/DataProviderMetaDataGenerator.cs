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
                TableAlias = tableName,
                IsSoftDeleteEnabled = SoftDeleteAttribute.IsEnabled(type, inherit: false)
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
                    IsCustomPrimaryKey = PrimaryKeyAttribute.IsPrimaryKey(info),
                    Name = info.Name,
                    ParameterName = info.Name,
                    PropertyInfo = info,
                    NonGenericType = IsNullableType(info) ? Nullable.GetUnderlyingType(info.PropertyType) : info.PropertyType
                };
            }

            if(infos.None(t => PrimaryKeyAttribute.IsPrimaryKey(t)))
            {
                var info = type.GetProperty(PropertyData.DEFAULT_ID_COLUMN);
                yield return new PropertyData
                {
                    Name = PropertyData.DEFAULT_ID_COLUMN,
                    ParameterName = PropertyData.DEFAULT_ID_COLUMN,
                    PropertyInfo = info,
                    NonGenericType = info.PropertyType,
                    IsDefaultId = true
                };
            }

            if (SoftDeleteAttribute.IsEnabled(type, inherit: false))
            {
                yield return new PropertyData
                {
                    Name = ".Deleted",
                    ParameterName = "_Deleted",
                    PropertyInfo = type.GetProperty("IsMarkedSoftDeleted"),
                    NonGenericType = typeof(bool),
                    IsDeleted = true
                };
            }

            yield return new PropertyData
            {
                Name = PropertyData.ORIGINAL_ID,
                ParameterName = PropertyData.ORIGINAL_ID,
                PropertyInfo = type.GetProperty(PropertyData.ORIGINAL_ID),
                NonGenericType = type.GetProperty(PropertyData.ORIGINAL_ID).PropertyType,
                IsOriginalId = true
            };
        }

        static Type[] GetDrivedClasses(Type type) =>
            type.Assembly.GetTypes().Where(t => t.IsA(type) && t != type).ToArray();

        static Type[] GetParents(Type type)
        {
            var result = new List<Type>();

            if (type.BaseType.IsAnyOf(null,
                typeof(object),
                typeof(GuidEntity),
                typeof(IntEntity),
                typeof(StringEntity))) return result.ToArray();

            result.AddRange(GetParents(type.BaseType));
            result.Add(type.BaseType);

            return result.ToArray();
        }
        static bool IsNullableType(PropertyInfo property)
        {
            return property.PropertyType.IsGenericType && 
                property.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
    }
}