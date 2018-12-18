using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Olive.Entities.Replication
{
    public abstract class ExposedField
    {
        protected string title, name;
        protected Type type;
        public bool IsAssociation { get; set; }

        public ExposedField Title(string title) { this.title = title; return this; }

        public ExposedField Name(string name) { this.name = name; return this; }

        public ExposedField Type(Type propertyType) { type = propertyType; return this; }

        public string GetTitle() => title;

        public string GetName() => name;

        public Type GetPropertyType() => type;

        protected abstract Task<object> GetValue(IEntity entity);

        public async Task<object> GetSerializableValue(IEntity entity)
        {
            var result = await GetValue(entity);
            if (result is IEntity ent) return ent.GetId();
            else return result;
        }

        protected internal virtual bool ShouldSerialize() => true;
    }

    public class CustomExposedField : ExposedField
    {
        Func<IEntity, Task<object>> ValueProvider;

        internal CustomExposedField(string title, Type propertyType, Func<IEntity, Task<object>> valueProvider)
        {
            this.title = title;
            type = propertyType;
            name = title.ToPascalCaseId();
            ValueProvider = valueProvider;
        }

        protected override Task<object> GetValue(IEntity entity) => ValueProvider(entity);
    }

    public class ExposedPropertyInfo : ExposedField
    {
        public PropertyInfo Property { get; }
        public bool IsInverseAssociation { get; }

        public ExposedPropertyInfo(PropertyInfo property)
        {
            Property = property;
            type = property.PropertyType;
            IsAssociation = type.IsA<IEntity>();
            IsInverseAssociation = type.IsA<IDatabaseQuery>() && property.Defines<CalculatedAttribute>();
            name = property.Name;

            title = property.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>()?.DisplayName;
            if (title.IsEmpty()) title = name.ToLiteralFromPascalCase();
        }

        protected override async Task<object> GetValue(IEntity entity)
        {
            var result = Property.GetValue(entity);

            if (result is Task t)
            {
                await t;
                result = t.GetType().GetProperty("Result").GetValue(t);
            }

            return result;
        }

        protected internal override bool ShouldSerialize() => !IsInverseAssociation;
    }
}