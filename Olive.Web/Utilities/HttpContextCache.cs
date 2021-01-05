namespace Olive.Web
{
    using System;

    /// <summary>
    /// Provides a HttpRequest level cache of objects.
    /// </summary>
    public static class HttpContextCache
    {
        /// <summary>
        /// Gets a specified cached value from the current HttpContext.
        /// If it doesn't exist, it will evaluate the provider expression to produce the value, adds it to cache, and returns it.
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(TKey key, Func<TValue> valueProducer)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (valueProducer == null) throw new ArgumentNullException(nameof(valueProducer));

            var bag = Olive.Context.Current.Http().Items;
            if (bag == null) return valueProducer();

            if (!bag.TryGetValue(key, out object value))
            {
                value = valueProducer();

                try { bag.Add(key, value); }
                catch (ArgumentException) { return GetOrAdd(key, valueProducer); }
            }

            return (TValue)value;
        }

        /// <summary>
        /// Removes a specified cached object by its key from the current Http Context.
        /// </summary>
        public static void Remove<TKey>(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var bag = Olive.Context.Current.Http()?.Items;

            if (bag?.ContainsKey(key) == true) bag.Remove(key);
        }
    }
}
