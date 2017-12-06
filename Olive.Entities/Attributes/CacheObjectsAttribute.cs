namespace Olive.Entities
{
    /// <summary>
    /// Specifies if a type is cacheable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class CacheObjectsAttribute : Attribute
    {
        static object DetectAndCacheShouldBeStaticMethod = new object();

        static Dictionary<Type, bool?> Cache = new Dictionary<Type, bool?>();

        static object SyncLock = new object();

        bool Enabled;

        /// <summary>
        /// Creates a new CacheObjectsAttribute instance.
        /// </summary>
        public CacheObjectsAttribute(bool enabled) => Enabled = enabled;

        /// <summary>
        /// Determines if caching is enabled for a given type.
        /// </summary>
        public static bool? IsEnabled(Type type)
        {
            if (Cache.TryGetValue(type, out bool? result)) return result;

            return DetectAndCache(type);
        }

        static bool? DetectAndCache(Type type)
        {
            lock (DetectAndCacheShouldBeStaticMethod)
            {
                var usage = type.GetCustomAttributes<CacheObjectsAttribute>(inherit: true).FirstOrDefault();

                var result = default(bool?);

                if (usage != null) result = usage.Enabled;

                try { return Cache[type] = result; }
                catch { return result; }
            }
        }
    }
}