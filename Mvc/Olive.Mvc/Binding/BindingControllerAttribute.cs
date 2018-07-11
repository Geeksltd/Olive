using System;
using System.Collections.Generic;
using System.Text;

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