using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Olive.Entities.ObjectDataProvider
{
    internal static class ObjectDataProviderExtension
    {
        internal static DbType ToDbType(this Type @this)
        {
            var typeMap = new Dictionary<Type, DbType>
            {
                [typeof(byte)] = DbType.Byte,
                [typeof(sbyte)] = DbType.SByte,
                [typeof(short)] = DbType.Int16,
                [typeof(ushort)] = DbType.UInt16,
                [typeof(int)] = DbType.Int32,
                [typeof(uint)] = DbType.UInt32,
                [typeof(long)] = DbType.Int64,
                [typeof(ulong)] = DbType.UInt64,
                [typeof(float)] = DbType.Single,
                [typeof(double)] = DbType.Double,
                [typeof(decimal)] = DbType.Decimal,
                [typeof(bool)] = DbType.Boolean,
                [typeof(string)] = DbType.String,
                [typeof(char)] = DbType.StringFixedLength,
                [typeof(Guid)] = DbType.Guid,
                [typeof(DateTime)] = DbType.DateTime,
                [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                [typeof(byte[])] = DbType.Binary,
                [typeof(byte?)] = DbType.Byte,
                [typeof(sbyte?)] = DbType.SByte,
                [typeof(short?)] = DbType.Int16,
                [typeof(ushort?)] = DbType.UInt16,
                [typeof(int?)] = DbType.Int32,
                [typeof(uint?)] = DbType.UInt32,
                [typeof(long?)] = DbType.Int64,
                [typeof(ulong?)] = DbType.UInt64,
                [typeof(float?)] = DbType.Single,
                [typeof(double?)] = DbType.Double,
                [typeof(decimal?)] = DbType.Decimal,
                [typeof(bool?)] = DbType.Boolean,
                [typeof(char?)] = DbType.StringFixedLength,
                [typeof(Guid?)] = DbType.Guid,
                [typeof(DateTime?)] = DbType.DateTime,
                [typeof(DateTimeOffset?)] = DbType.DateTimeOffset
            };

            if (@this != null && typeMap.ContainsKey(@this))
                return typeMap[@this];
            else
                return DbType.String;
        }

        internal static bool IsNumericType(this Type mytype)
        {
            switch (Type.GetTypeCode(mytype))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsAccociation(this PropertyInfo myProperty)
        {
            if (myProperty != null)
                if (myProperty.DeclaringType != myProperty.PropertyType && myProperty.DeclaringType.Namespace == myProperty.PropertyType.Namespace)
                    return true;
            return false;
        }

        internal static Dictionary<string, Property> GetDatabaseFieldsList(this EntityType @this)
        {
            var result = new Dictionary<string, Property>();

            foreach (var property in @this.Properties.Where(i => i.HasDatabaseRepresentation).OrderBy(i => i.Order))
            {
                if (property is Association)
                {
                    var association = property as Association;

                    if (!association.IsManyToMany && !association.IsCalculated)
                        result.Add(association.DatabaseColumnName, association);
                }
                else
                {
                    result.Add(property.DatabaseColumnName, property);
                }
            }

            return result;
        }

        internal static bool CanSaveId(this EntityType type)
        {
            if (type.HasCustomPrimaryKeyColumn) return false;
            if (type.AssignIDByDatabase) return false;

            return true;
        }

        internal static Property FindAutoNumber(this EntityType type)
        {
            if (type.AssignIDByDatabase)
                return new NumberProperty { Name = "ID", SpecifiedPropertyType = type.PrimaryKeyType, IsMandatory = true, DatabaseColumnName = "ID", PropertyType = type.PrimaryKeyType };

            return type.Properties.OfType<NumberProperty>().FirstOrDefault(i => i.IsAutoNumber && i.HasDatabaseRepresentation);
        }

        internal static List<string> GetSaveSqlFields(this EntityType type)
        {
            var result = new List<string>(); ;

            if (type.CanSaveId()) result.Add("Id");

            result.AddRange(type.GetDatabaseFieldsList()
                .Except(f => (f.Value as NumberProperty)?.IsAutoNumber == true)
                .Except(f => (f.Value as StringProperty)?.IsRowVersion == true)
                .Select(x => $"[{x.Key}]"));

            if (type.SoftDelete) result.Add("[.DELETED]");

            return result;
        }

        internal static bool TypeIsNotCorrect(this Type runtimeType)
        {
            if (runtimeType.IsAnyOf(null, 
                typeof(object), 
                typeof(GuidEntity), 
                typeof(IntEntity), 
                typeof(StringEntity)))
                return true;

            return false;
        }
    }
}