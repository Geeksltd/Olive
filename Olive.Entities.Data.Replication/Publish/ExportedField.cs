using System.Reflection;

namespace Olive.Entities.Replication
{
    public class ExportedField
    {
        string title;
        public PropertyInfo Property { get; }

        public bool IsAssociation => Property.PropertyType.IsA<IEntity>();

        public bool IsInverseAssociation => Property.PropertyType.IsA<IDatabaseQuery>() && Property.Defines<CalculatedAttribute>();

        public ExportedField(PropertyInfo property)
        {
            Property = property;
            title = property.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>()?.DisplayName;
            if (title.IsEmpty())
                title = property.Name.ToLiteralFromPascalCase();
        }

        public string GetTitle() => title;

        public ExportedField Title(string exportTitle)
        {
            title = exportTitle;
            return this;
        }

        public object GetValue(IEntity entity)
        {
            var result = Property.GetValue(entity);
            if (result is IEntity ent) return ent.GetId();
            else return result;
        }
    }
}