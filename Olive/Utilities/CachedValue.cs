using System;

namespace Olive
{
    public static class CachedValue
    {
        /// <summary>
        /// Creates a CachedValue the specified value builder.
        /// </summary>
        public static CachedValue<T> Create<T>(Func<T> valueBuilder) => new CachedValue<T>(valueBuilder);
    }

    public class CachedValue<T>
    {
        /// <summary>
        /// Stores the underlying value.
        /// </summary>
        T _Value;

        Func<T> ValueBuilder;

        /// <summary>
        /// Gets the underlying value.
        /// </summary>
        public T Value
        {
            get
            {
                if (ValueBuilder != null)
                {
                    _Value = ValueBuilder();
                    ValueBuilder = null;
                }

                return _Value;
            }
        }

        /// <summary>
        /// Creates a new CachedValue instance.
        /// </summary>
        public CachedValue(T value) => _Value = value;

        /// <summary>
        /// Initializes a new CachedValue instance with lazy loading support.
        /// </summary>
        /// <param name="valueBuilder">The value builder.</param>
        public CachedValue(Func<T> valueBuilder) => ValueBuilder = valueBuilder;

        public static implicit operator T(CachedValue<T> value) => value.Value;

        public static implicit operator CachedValue<T>(T value) => new CachedValue<T>(value);
    }
}
