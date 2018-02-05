using System;
using System.Collections.Generic;
using System.Text;

namespace Olive
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DataProviderAttribute : Attribute
    {
        public Type EntityType;
        public DataProviderAttribute(Type entityType)
        {
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        }
    }
}
