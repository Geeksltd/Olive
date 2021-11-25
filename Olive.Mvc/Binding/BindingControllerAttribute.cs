using System;

namespace Olive.Mvc
{
    public class BindingControllerAttribute : Attribute
    {
        public Type Type { get; }
        public BindingControllerAttribute(Type type)
        {
            if (!type.IsA<Controller>())
                throw new ArgumentException(type.FullName + " is not a Controller type.");

            Type = type;
        }
    }
}