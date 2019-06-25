using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Data
{
    internal partial class DataProviderMetaDataGenerator
    {
        internal static IDataProviderMetaData Generate(Type type)
        {
            var tableName = TableNameAttribute.GetTableName(type);

            return new DataProviderMetaData(type)
            {
                BaseClassTypesInOrder = GetParents(type),
                DrivedClassTypes = GetDrivedClasses(type),
                Properties = GetProperties(type).SetAccessors(type),
                Schema = SchemaAttribute.GetSchema(type),
                TableName = tableName,
                TableAlias = tableName,
                IsSoftDeleteEnabled = SoftDeleteAttribute.IsEnabled(type, inherit: false)
            };
        }

        static IEnumerable<PropertyData> GetProperties(Type type)
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

        static PropertyData GetDefaultIdPropertyData(Type type)
        {
            var info = type.GetProperty(PropertyData.DEFAULT_ID_COLUMN);

            return new PropertyData
            {
                Name = PropertyData.DEFAULT_ID_COLUMN,
                ParameterName = PropertyData.DEFAULT_ID_COLUMN,
                PropertyInfo = info,
                NonGenericType = info.PropertyType,
                IsDefaultId = true,
                IsAutoNumber = IdByDatabaseAttribute.IsIdAssignedByDatabase(type)
            };
        }

        static PropertyData GetDeletedPropertyData(Type type)
        {
            return new PropertyData
            {
                Name = ".Deleted",
                ParameterName = "_Deleted",
                PropertyInfo = type.GetProperty("IsMarkedSoftDeleted"),
                NonGenericType = typeof(bool),
                IsDeleted = true
            };
        }

        static PropertyData GetOriginalIdPropertyData(Type type)
        {
            return new PropertyData
            {
                Name = PropertyData.ORIGINAL_ID,
                ParameterName = PropertyData.ORIGINAL_ID,
                PropertyInfo = type.GetProperty(PropertyData.ORIGINAL_ID),
                NonGenericType = type.GetProperty(PropertyData.ORIGINAL_ID).PropertyType,
                IsOriginalId = true
            };
        }

        static PropertyData GetUserDefinedPropertyData(PropertyInfo info, PropertyInfo dbProp)
        {
            var isBlob = info.PropertyType.IsA<Blob>();
            var targetProp = dbProp ?? info;
            var columnName = info.Name + "_FileName".OnlyWhen(isBlob);

            return new PropertyData
            {
                IsAutoNumber = AutoNumberAttribute.IsAutoNumber(info),
                IsCustomPrimaryKey = PrimaryKeyAttribute.IsPrimaryKey(info),
                Name = columnName,
                ParameterName = columnName,
                PropertyInfo = targetProp,
                NonGenericType = IsNullableType(targetProp) ? Nullable.GetUnderlyingType(targetProp.PropertyType) : targetProp.PropertyType,
                AssociateType = dbProp != null ? info.GetAssociateType() : null,
                CustomDataConverterClassName = CustomDataConverterAttribute.GetClassName(targetProp)
            };
        }

        static Type GetAssociateType(this PropertyInfo @this)
        {
            if (@this.PropertyType.IsA<Task>())
                return @this.PropertyType.GenericTypeArguments[0];

            return @this.PropertyType;
        }

        static bool IsAssociation(this PropertyInfo @this)
        {
            if (@this.PropertyType.IsA<IEntity>()) return true;

            if (@this.PropertyType.IsA<Task>() &&
                @this.PropertyType.IsGenericType &&
                @this.PropertyType.GenericTypeArguments[0].IsA<IEntity>()) return true;

            return false;
        }

        static IEnumerable<(PropertyInfo MainInfo, PropertyInfo DatabaseProp)> FilterProperties(PropertyInfo[] infos)
        {
            var nonCalculated = infos.Except(p => CalculatedAttribute.IsCalculated(p) || p.GetSetMethod() == null);
            var nonOverriden = nonCalculated.Except(p => p.GetGetMethod() != p.GetGetMethod().GetBaseDefinition());
            var nonTransient = nonOverriden.Except(p => TransientEntityAttribute.IsTransient(p.PropertyType));

            var associations = nonTransient.Where(p => p.IsAssociation());
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

        static Type[] GetDrivedClasses(Type type)
        {
            var result = type.Assembly.GetTypes().Where(t => t.IsA(type) && t != type && !TransientEntityAttribute.IsTransient(t)).ToArray();

            Array.Sort(result, new TypeComparer(type));

            return result;
        }

        class TypeComparer : IComparer<Type>
        {
            readonly Type Type;

            public TypeComparer(Type type) => Type = type;

            public int Compare(Type left, Type right) =>
                CountBaseType(left).CompareTo(CountBaseType(right));

            int CountBaseType(Type type)
            {
                if (type == Type) return 0;

                return CountBaseType(type.BaseType) + 1;
            }
        }


        static Type[] GetParents(Type type)
        {
            var result = new List<Type>();
            var baseType = type.BaseType;

            if (baseType.IsAnyOf(null,
                typeof(object),
                typeof(GuidEntity),
                typeof(IntEntity),
                typeof(StringEntity)) ||
                (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Entity<>)))
                return result.ToArray();

            result.AddRange(GetParents(baseType));
            result.Add(baseType);

            return result.ToArray();
        }

        static bool IsNullableType(PropertyInfo property)
        {
            return property.PropertyType.IsGenericType &&
                property.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
    }
}