namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Performs a specified action on this item if it is not null. If it is null, it simply ignores the action.
        /// </summary>
        public static void Perform<T>(this T item, Action<T> action) where T : class
        {
            if (item != null) action?.Invoke(item);
        }

        /// <summary>
        /// Performs a specified action on this item if it is not null. If it is null, it simply ignores the action.
        /// </summary>
        public static async Task Perform<T>(this T item, Func<T, Task> func) where T : class
        {
            if (item != null) await func?.Invoke(item);
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static K Get<T, K>(this T item, Func<T, K> selector)
        {
            if (object.ReferenceEquals(item, null))
                return default(K);
            else
            {
                try
                {
                    return selector(item);
                }
                catch (NullReferenceException)
                {
                    return default(K);
                }
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static K? Get<T, K>(this T item, Func<T, K?> selector) where K : struct
        {
            if (item == null) return null;

            try
            {
                return selector(item);
            }
            catch (NullReferenceException)
            {
                return default(K?);
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static K Get<K>(TimeSpan? item, Func<TimeSpan, K> selector)
        {
            if (item == null) return default(K);

            try
            {
                return selector(item.Value);
            }
            catch (NullReferenceException)
            {
                return default(K);
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static Guid? Get<T>(this T item, Func<T, Guid> selector) where T : class
        {
            if (item == null) return null;

            try
            {
                return selector(item);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static int? Get<T>(this T item, Func<T, int> selector) where T : class
        {
            if (item == null) return null;

            try
            {
                return selector(item);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static double? Get<T>(this T item, Func<T, double> selector) where T : class
        {
            if (item == null) return null;

            try
            {
                return selector(item);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static decimal? Get<T>(this T item, Func<T, decimal> selector) where T : class
        {
            if (item == null) return null;

            try
            {
                return selector(item);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static bool? Get<T>(this T item, Func<T, bool> selector) where T : class
        {
            if (item == null) return null;

            try
            {
                return selector(item);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static string Get(this DateTime? item, Func<DateTime?, string> selector)
        {
            if (item == null) return null;

            try
            {
                return selector?.Invoke(item);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static byte? Get<T>(this T item, Func<T, byte> selector) where T : class
        {
            if (item == null) return null;

            try
            {
                return selector(item);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static DateTime? Get<T>(this T item, Func<T, DateTime> selector) where T : class
        {
            if (item == null) return null;

            try
            {
                return selector(item);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static DateTime? Get<T>(this T item, Func<T, DateTime?> selector) where T : class
        {
            if (item == null) return null;

            try
            {
                return selector(item);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a specified member of this object. If this is null, null will be returned. Otherwise the specified expression will be returned.
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public static T Get<T>(this DateTime? item, Func<DateTime?, T> selector) where T : struct
        {
            if (item == null) return default(T);

            try
            {
                return selector(item);
            }
            catch (NullReferenceException)
            {
                return default(T);
            }
        }
    }
}
