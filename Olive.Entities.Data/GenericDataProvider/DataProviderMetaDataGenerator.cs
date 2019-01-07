using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Olive.Entities.Data
{
    internal class DataProviderMetaDataGenerator
    {
        internal static IDataProviderMetaData Generate(Type type)
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

        static IEnumerable<IPropertyData> GetProperties(Type type)
        {
            var infos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var (info, dbProp) in FilterProperties(infos))
                yield return GetUserDefinedPropertyData(info, dbProp);

            if (infos.None(t => PrimaryKeyAttribute.IsPrimaryKey(t)))
                yield return GetDefaultIdPropertyData(type);

            if (SoftDeleteAttribute.IsEnabled(type, inherit: false))
                yield return GetDeletedPropertyData(type);

            yield return GetOriginalIdPropertyData(type);
        }

        static IPropertyData GetDefaultIdPropertyData(Type type)
        {
            var info = type.GetProperty(PropertyData.DEFAULT_ID_COLUMN);

            return new PropertyData(isBlob: false)
            {
                Name = PropertyData.DEFAULT_ID_COLUMN,
                ParameterName = PropertyData.DEFAULT_ID_COLUMN,
                PropertyInfo = info,
                NonGenericType = info.PropertyType,
                IsDefaultId = true
            };
        }

        static IPropertyData GetDeletedPropertyData(Type type)
        {
            return new PropertyData(isBlob: false)
            {
                Name = ".Deleted",
                ParameterName = "_Deleted",
                PropertyInfo = type.GetProperty("IsMarkedSoftDeleted"),
                NonGenericType = typeof(bool),
                IsDeleted = true
            };
        }

        static IPropertyData GetOriginalIdPropertyData(Type type)
        {
            return new PropertyData(isBlob: false)
            {
                Name = PropertyData.ORIGINAL_ID,
                ParameterName = PropertyData.ORIGINAL_ID,
                PropertyInfo = type.GetProperty(PropertyData.ORIGINAL_ID),
                NonGenericType = type.GetProperty(PropertyData.ORIGINAL_ID).PropertyType,
                IsOriginalId = true
            };
        }

        static IPropertyData GetUserDefinedPropertyData(PropertyInfo info, PropertyInfo dbProp)
        {
            var isBlob = info.PropertyType.IsA<Blob>();
            var targetProp = dbProp ?? info;
            var columnName = info.Name + "_FileName".OnlyWhen(isBlob);

            return new PropertyData(isBlob)
            {
                IsAutoNumber = AutoNumberAttribute.IsAutoNumber(info),
                IsCustomPrimaryKey = PrimaryKeyAttribute.IsPrimaryKey(info),
                Name = columnName,
                ParameterName = columnName,
                PropertyInfo = targetProp,
                NonGenericType = IsNullableType(targetProp) ? Nullable.GetUnderlyingType(targetProp.PropertyType) : targetProp.PropertyType,
                AssociateType = dbProp != null ? info.PropertyType : null
            };
        }

        static IEnumerable<(PropertyInfo MainInfo, PropertyInfo DatabaseProp)> FilterProperties(PropertyInfo[] infos)
        {
            var nonCalculated = infos.Except(p => CalculatedAttribute.IsCalculated(p) || p.GetSetMethod() == null);
            var nonOverriden = nonCalculated.Except(p => p.GetGetMethod() != p.GetGetMethod().GetBaseDefinition());
            var nonTransient = nonOverriden.Except(p => TransientEntityAttribute.IsTransient(p.PropertyType));

            var associations = nonTransient.Where(predicate => predicate.PropertyType.IsA<IEntity>());
            var rest = nonTransient.Except(associations);

            var ids = new List<PropertyInfo>();

            PropertyInfo getIdFor(PropertyInfo info)
            {
                var result = rest.FirstOrDefault(p => p.Name == info.Name.WithSuffix("Id"));
                ids.Add(result);
                return result;
            }

            foreach (var prop in associations)
                yield return (prop, getIdFor(prop));

            foreach (var prop in rest.Except(ids))
                yield return (prop, null);
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