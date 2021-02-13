using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Olive
{
    internal class PropertyComparer : IComparer
    {
        readonly PropertyInfo Property;

        public PropertyComparer(PropertyInfo property) => Property = property;

        public TValue ExtractValue<TItem, TValue>(TItem item) => (TValue)Property.GetValue(item, null);

        public int Compare(object first, object second) =>
            Comparer.Default.Compare(Property.GetValue(first, null), Property.GetValue(second, null));
    }

    internal class PropertyComparer<T> : IComparer<T>
    {
        readonly PropertyInfo Property;

        public PropertyComparer(PropertyInfo property) => Property = property;

        public TValue ExtractValue<TItem, TValue>(TItem item) => (TValue)Property.GetValue(item, null);

        public int Compare(T first, T second) =>
            Comparer<T>.Default.Compare((T)Property.GetValue(first, null), (T)Property.GetValue(second, null));
    }
}
