namespace Olive.Entities.Data
{
    using Olive;
    using Olive.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public abstract class EncryptedEntityInterceptor
    {
        internal static string EncryptionKey;
        internal static Dictionary<Type, PropertyInfo[]> EncryptedProperties;

        public static void Initialize(Assembly domainAssembly)
        {
            if (EncryptionKey.HasValue())
                throw new InvalidOperationException("Inizialize");

            EncryptionKey = Config.GetOrThrow("Database:DataEncryption:Key");

            var domainEntities = domainAssembly.GetTypes().Where(t => t.IsA<Entity>());

            EncryptedProperties = domainEntities
                .ToDictionary(x => x, x => FindEncryptedProperties(x));
        }

        static PropertyInfo[] FindEncryptedProperties(Type type)
        {
            var all = BindingFlags.FlattenHierarchy | BindingFlags.Public
                | BindingFlags.GetProperty | BindingFlags.Instance;

            var result = type.GetProperties(all)
                             .Where(p => p.Defines<EncryptedPropertyAttribute>())
                             .ToArray();

            foreach (var p in result.Where(x => x.PropertyType != typeof(string)))
            {
                throw new InvalidOperationException($"[EncryptedProperty] can only be applied to string properties. Invalid: {p.DeclaringType.FullName + "." + p.Name}");
            }

            return result;
        }

        protected Task Process(IEntity entity, Func<string, string, string> valueConverter)
        {
            var properties = EncryptedProperties.GetOrDefault(entity.GetType());
            if (properties.None()) return Task.CompletedTask;

            foreach (var property in properties)
            {
                var clean = (string)property.GetValue(entity);
                if (clean.HasValue())
                    property.SetValue(entity, valueConverter(clean, EncryptionKey));
            }

            return Task.CompletedTask;
        }
    }
}