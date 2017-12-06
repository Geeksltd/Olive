namespace Olive.Entities
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ManyToManyAttribute : Attribute
    {
        static ConcurrentDictionary<Tuple<Type, bool?>, PropertyInfo[]> Cache = new ConcurrentDictionary<Tuple<Type, bool?>, PropertyInfo[]>();

        /// <summary>
        /// Gets or sets the Lazy of this ManyToManyAttribute.
        /// </summary>
        public bool Lazy { get; set; }

        /// <summary>
        /// Gets a list of types that depend on a given entity.
        /// </summary>
        public static IEnumerable<PropertyInfo> GetManyToManyProperties(Type type) => GetManyToManyProperties(type, lazy: null);

        /// <summary>
        /// Gets a list of types that depend on a given entity.
        /// </summary>
        public static IEnumerable<PropertyInfo> GetManyToManyProperties(Type type, bool? lazy)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var key = Tuple.Create(type, lazy);

            return Cache.GetOrAdd(key, x => FindManyToManyProperties(x.Item1, x.Item2));
        }

        /// <summary>
        /// Returns a list of types that depend on a given entity.
        /// </summary>
        static PropertyInfo[] FindManyToManyProperties(Type type, bool? lazy)
        {
            return (from p in type.GetProperties()
                    let att = p.GetCustomAttribute<ManyToManyAttribute>(inherit: true)
                    where att != null
                    where lazy == null || att.Lazy == lazy
                    select p).Distinct().ToArray();
        }
    }
}