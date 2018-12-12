using System;
using System.Reflection;

namespace Olive.Entities.Replication
{
    public abstract class ExportedField
    {
        protected string title, name;
        protected Type type;
        public bool IsAssociation { get; set; }

        public ExportedField Title(string exportTitle) { title = exportTitle; return this; }

        public ExportedField Name(string exportName) { name = exportName; return this; }

        public ExportedField Type(Type propertyType) { type = propertyType; return this; }

        public string GetTitle() => title;

        public string GetName() => name;

        public Type GetPropertyType() => type;

        protected abstract object GetValue(IEntity entity);

        public object GetSerializableValue(IEntity entity)
        {
            var result = GetValue(entity);
            if (result is IEntity ent) return ent.GetId();
            else return result;
        }

        protected internal virtual bool ShouldSerialize() => true;
    }

    public class CustomExportedField : ExportedField
    {
        Func<IEntity, object> ValueProvider;

        internal CustomExportedField(string title, Type propertyType, Func<IEntity, object> valueProvider)
        {
            this.title = title;
            type = propertyType;
            name = title.ToPascalCaseId();
            ValueProvider = valueProvider;
        }

        protected override object GetValue(IEntity entity) => ValueProvider(entity);
    }

    public class ExportedPropertyInfo : ExportedField
    {
        public PropertyInfo Property { get; }
        public bool IsInverseAssociation { get; }

        public ExportedPropertyInfo(PropertyInfo property)
        {
            Property = property;
            type = property.PropertyType;
            IsAssociation = type.IsA<IEntity>();
            IsInverseAssociation = type.IsA<IDatabaseQuery>() && property.Defines<CalculatedAttribute>();
            name = property.Name;

            title = property.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>()?.DisplayName;
            if (title.IsEmpty()) title = name.ToLiteralFromPascalCase();
        }

        protected override object GetValue(IEntity entity) => Property.GetValue(entity);

        protected internal override bool ShouldSerialize() => !IsInverseAssociation;
    }
}