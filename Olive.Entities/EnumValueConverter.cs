using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Entities
{
#pragma warning disable IDE1006 // Naming Styles
    public interface EnumValueConverter { }
#pragma warning restore IDE1006 // Naming Styles

    public class EnumValueConverter<T> : EnumValueConverter, IValueConverter<T>
        where T : struct
    {
        public object ConvertFrom(T value) => (int)(dynamic)value;

        public T ConvertTo(object value) => (T)value;
    }
}
