using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Entities
{
    public interface IValueConverter<T>
    {
        T ConvertTo(object value);
        object ConvertFrom(T value);
    }
}
