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
        public static void Add<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> items)
        {
            foreach (var item in items)
                dictionary.Add(item.Key, item.Value);
        }

        public static void RemoveWhere<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            Func<KeyValuePair<TKey, TValue>, bool> selector)
        {
            lock (dictionary)
            {
                var toRemove = dictionary.Where(selector).ToList();

                foreach (var item in toRemove) dictionary.Remove(item.Key);
            }
        }

        public static void RemoveWhereKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<TKey, bool> selector)
        {
            lock (dictionary)
            {
                var toRemove = dictionary.Where(x => selector(x.Key)).ToList();

                foreach (var item in toRemove) dictionary.Remove(item.Key);
            }
        }

        /// <summary>
        /// Converts this to a KeyValueCollection.
        /// </summary>
        public static NameValueCollection ToNameValueCollection<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            var result = new NameValueCollection();

            foreach (var item in dictionary)
                result.Add(item.Key.ToStringOrEmpty(), item.Value.ToStringOrEmpty());

            return result;
        }

        public static HttpContent AsHttpContent(this IDictionary<string, string> dictionary)
        {
            HttpContent content = new FormUrlEncodedContent(dictionary);
            return content;
        }
        /// <summary>
        /// Adds the properties of a specified [anonymous] object as items to this dictionary.
        /// It ignores duplicate entries and null values.
        /// </summary>
        public static Dictionary<string, TValue> AddFromProperties<TValue>(this Dictionary<string, TValue> dictionary, object data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            foreach (var property in data.GetType().GetProperties())
            {
                if (dictionary.ContainsKey(property.Name)) continue;

                var value = property.GetValue(data);

                if (value == null) continue;

                if (typeof(TValue) == typeof(string) && value.GetType() != typeof(string))
                    value = value.ToStringOrEmpty();

                if (!value.GetType().IsA(typeof(TValue)))
                {
                    throw new Exception("The value on property '{0}' is of type '{1}' which cannot be casted as '{2}'."
                        .FormatWith(property.Name, value.GetType().FullName, typeof(TValue).FullName));
                }

                dictionary.Add(property.Name, (TValue)value);
            }

            return dictionary;
        }

        /// <summary>
        /// Gets the keys of this dictionary.
        /// </summary>
        public static IEnumerable<T> GetKeys<T, K>(this IDictionary<T, K> dictionary) => dictionary.Select(i => i.Key);

        /// <summary>
        /// Gets all values from this dictionary.
        /// </summary>
        public static IEnumerable<TValue> GetAllValues<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            foreach (var item in dictionary)
                yield return item.Value;
        }

        /// <summary>
        /// Tries to the remove an item with the specified key from this dictionary.
        /// </summary>
        public static K TryRemove<T, K>(this System.Collections.Concurrent.ConcurrentDictionary<T, K> list, T key)
        {
            if (list.TryRemove(key, out var result))
                return result;
            else return default(K);
        }

        /// <summary>
        /// Tries to the remove an item with the specified key from this dictionary.
        /// </summary>
        public static K TryRemoveAt<T, K>(this System.Collections.Concurrent.ConcurrentDictionary<T, K> list, int index)
        {
            try
            {
                return list.TryRemove(list.Keys.ElementAt(index));
            }
            catch
            {
                // No logging is needed
                return default(K);
            }
        }

        /// <summary>
        /// Gets all values from this dictionary.
        /// </summary>
        public static IEnumerable<TValue> GetAllValues<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dictionary)
        {
            foreach (var item in dictionary)
                yield return item.Value;
        }

        public static bool LacksKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) =>
            !dictionary.ContainsKey(key);

        public static bool Lacks(this IDictionary dictionary, object key) => !dictionary.Contains(key);

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            var keyType = typeof(TKey);

            if (keyType.IsValueType || keyType == typeof(string) || keyType == typeof(Type))
            {
                if (dictionary.TryGetValue(key, out var result)) return result;
                return default(TValue);
            }
            else
            {
                return dictionary.FirstOrDefault(x => x.Key.Equals(key)).Value;
            }
        }

        /// <summary>
        /// Gets the value with the specified key, or null.
        /// </summary>
        public static TValue TryGet<TKey, TValue>(this IDictionary<TKey, TValue> list, TKey key)
        {
            if (list.TryGetValue(key, out var result)) return result;

            return default(TValue);
        }

        /// <summary>
        /// Adds the specified types pair to this type dictionary.
        /// </summary>
        public static void Add<T, K>(this IDictionary<Type, Type> typeDictionary) => typeDictionary.Add(typeof(T), typeof(K));
    }
}