using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Adds all items from a specified dictionary to this dictionary.
        /// </summary>
        public static void Add<TKey, TValue>(this IDictionary<TKey, TValue> @this, IDictionary<TKey, TValue> items)
        {
            foreach (var item in items)
                @this.Add(item.Key, item.Value);
        }

        public static void RemoveWhere<TKey, TValue>(
            this IDictionary<TKey, TValue> @this,
            Func<KeyValuePair<TKey, TValue>, bool> selector)
        {
            lock (@this)
            {
                var toRemove = @this.Where(selector).ToList();

                foreach (var item in toRemove) @this.Remove(item.Key);
            }
        }

        public static void RemoveWhereKey<TKey, TValue>(this IDictionary<TKey, TValue> @this, Func<TKey, bool> selector)
        {
            lock (@this)
            {
                var toRemove = @this.Where(x => selector(x.Key)).ToList();

                foreach (var item in toRemove) @this.Remove(item.Key);
            }
        }

        /// <summary>
        /// Converts this to a KeyValueCollection.
        /// </summary>
        public static NameValueCollection ToNameValueCollection<TKey, TValue>(this IDictionary<TKey, TValue> @this)
        {
            var result = new NameValueCollection();

            foreach (var item in @this)
                result.Add(item.Key.ToStringOrEmpty(), item.Value.ToStringOrEmpty());

            return result;
        }

        public static HttpContent AsHttpContent(this IDictionary<string, string> @this)
        {
            return new FormUrlEncodedContent(@this);
        }

        /// <summary>
        /// Adds the properties of a specified [anonymous] object as items to this dictionary.
        /// It ignores duplicate entries and null values.
        /// </summary>
        public static Dictionary<string, TValue> AddFromProperties<TValue>(this Dictionary<string, TValue> @this, object data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            foreach (var property in data.GetType().GetProperties())
            {
                if (@this.ContainsKey(property.Name)) continue;

                var value = property.GetValue(data);

                if (value == null) continue;

                if (typeof(TValue) == typeof(string) && value.GetType() != typeof(string))
                    value = value.ToStringOrEmpty();

                if (!value.GetType().IsA(typeof(TValue)))
                {
                    throw new Exception("The value on property '{0}' is of type '{1}' which cannot be casted as '{2}'."
                        .FormatWith(property.Name, value.GetType().FullName, typeof(TValue).FullName));
                }

                @this.Add(property.Name, (TValue)value);
            }

            return @this;
        }

        /// <summary>
        /// Gets the keys of this dictionary.
        /// </summary>
        public static IEnumerable<T> GetKeys<T, K>(this IDictionary<T, K> @this) => @this.Select(i => i.Key);

        /// <summary>
        /// Gets all values from this dictionary.
        /// </summary>
        public static IEnumerable<TValue> GetAllValues<TKey, TValue>(this IDictionary<TKey, TValue> @this)
        {
            foreach (var item in @this)
                yield return item.Value;
        }

        /// <summary>
        /// Tries to the remove an item with the specified key from this dictionary.
        /// </summary>
        public static K TryRemove<T, K>(this System.Collections.Concurrent.ConcurrentDictionary<T, K> @this, T key)
        {
            if (@this.TryRemove(key, out var result))
                return result;
            else return default;
        }

        /// <summary>
        /// Tries to the remove an item with the specified key from this dictionary.
        /// </summary>
        public static K TryRemoveAt<T, K>(this System.Collections.Concurrent.ConcurrentDictionary<T, K> @this, int index)
        {
            try
            {
                return @this.TryRemove(@this.Keys.ElementAt(index));
            }
            catch
            {
                // No logging is needed
                return default;
            }
        }

        /// <summary>
        /// Gets all values from this dictionary.
        /// </summary>
        public static IEnumerable<TValue> GetAllValues<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> @this)
        {
            foreach (var item in @this)
                yield return item.Value;
        }

        public static bool LacksKey<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key) =>
            !@this.ContainsKey(key);

        public static bool Lacks(this IDictionary @this, object key) => !@this.Contains(key);

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)
        {
            if (@this is null) return default;

            var keyType = typeof(TKey);

            if (keyType.IsValueType || keyType == typeof(string) || keyType == typeof(Type))
            {
                if (@this.TryGetValue(key, out var result)) return result;
                return default;
            }
            else
            {
                return @this.FirstOrDefault(x => x.Key.Equals(key)).Value;
            }
        }

        /// <summary>
        /// Gets the value with the specified key, or null.
        /// </summary>
        public static TValue TryGet<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)
        {
            if (@this is null) return default;
            if (@this.TryGetValue(key, out var result)) return result;

            return default;
        }

        /// <summary>
        /// Adds the specified types pair to this type dictionary.
        /// </summary>
        public static void Add<T, K>(this IDictionary<Type, Type> @this)
            => @this.Add(typeof(T), typeof(K));

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, Func<TValue> valueProvider)
        {
            if (dic.TryGetValue(key, out var result)) return result;
            result = valueProvider();
            lock (dic) dic[key] = result;
            return result;
        }

        /// <summary>
        /// Converts this key value pair list into a Json object.
        /// </summary>
        public static JObject ToJson(this IEnumerable<KeyValuePair<string, string>> @this)
        {
            var result = new JObject();
            foreach (var item in @this)
                result.Add(item.Key, item.Value);
            return result;
        }
    }
}