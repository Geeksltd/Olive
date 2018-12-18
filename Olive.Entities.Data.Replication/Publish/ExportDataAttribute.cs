using System;

namespace Olive.Entities.Replication
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ExportDataAttribute : Attribute
    {
        public Type Type { get; }

        public ExportDataAttribute(Type type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));

            if (!type.IsA<ExposedType>())
                throw new ArgumentException(type.FullName + " is not a subclass of " + typeof(ExposedType).FullName);
        }
    }
}