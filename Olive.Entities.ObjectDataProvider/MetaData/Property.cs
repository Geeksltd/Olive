using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;

namespace Olive.Entities.ObjectDataProvider
{
    class Property
    {
        public string Title, Name, PropertyType, DatabaseColumnName, DatabaseColumnType, ColumnIdentifier;

        public bool IsMandatory, ShouldConvertDatabaseValue,
            HasDefaultValue, HasDatabaseRepresentation, IsCalculated, IsPrimaryKey,
            IsNeedsDbNullCheck;

        public EntityType EntityType;

        public int Order;

        public DbType DbType;

        public void Initialize(PropertyInfo myProperty, string databaseColumnName, string databaseColumnType, DbType propertyDbType, int orderIndexOfPropertyItem)
        {
            Name = myProperty.Name;
            Title = myProperty.Name.ToLiteralFromPascalCase();
            PropertyType = myProperty.PropertyType.ToString();

            ColumnIdentifier = myProperty.Name;

            IsMandatory = myProperty.IsDefined(typeof(RequiredAttribute));
            HasDefaultValue = myProperty.IsDefined(typeof(DefaultValueAttribute));
            IsCalculated = myProperty.IsDefined(typeof(CalculatedAttribute));
            IsPrimaryKey = myProperty.IsDefined(typeof(PrimaryKeyAttribute));
            HasDatabaseRepresentation = !myProperty.IsDefined(typeof(CalculatedAttribute));
            ShouldConvertDatabaseValue = myProperty.IsDefined(typeof(NeedsCastingDatabaseValueAttribute));

            Order = orderIndexOfPropertyItem;
            DatabaseColumnName = databaseColumnName;
            DatabaseColumnType = databaseColumnType;
            DbType = propertyDbType;

            IsNeedsDbNullCheck = NeedsDbNullCheck();
        }

        bool CanBeDbNull()
        {
            if (!IsMandatory) return true;

            if (this is Association && (this as Association).IsManyToMany)
                return true;

            return false;
        }

        bool NeedsDbNullCheck()
        {
            if (!CanBeDbNull()) return false;
            if (PropertyType != "string") return true;
            // if (!Project.IsCore()) return true;
            if (ShouldConvertDatabaseValue) return true;

            return false;
        }
    }

    class DateTimeProperty : Property
    {
        public bool HasTime, HasDate, TimeOnly;
    }

    class BinaryProperty : Property { }

    class BooleanProperty : Property { }

    class NumberProperty : Property
    {
        public bool IsAutoNumber;
        public string SpecifiedPropertyType;
    }

    class StringProperty : Property
    {
        public bool IsRowVersion;
    }

    class Association : Property
    {
        public EntityType ReferencedType;
        public bool IsManyToOne, IsManyToMany, LazyLoad;

        public string BridgeColumn1, BridgeColumn2, BridgeTableName;

        public Association InverseAssociation;
    }
}