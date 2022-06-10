using System;
using System.Collections.Generic;

namespace Olive.Entities.Data
{
    public class DynamoDBDataProviderFactory : IDataProviderFactory
    {
        static readonly Dictionary<Type, IDataProvider> ProviderCache = new();

        public string ConnectionString => throw new NotImplementedException();

        public DynamoDBDataProviderFactory(Type type) : this(new DatabaseConfig.ProviderMapping { Type = type }) { }

        public DynamoDBDataProviderFactory(DatabaseConfig.ProviderMapping _) { }

        public IDataAccess GetAccess() => throw new NotImplementedException();

        public IDataProvider GetProvider(Type type)
        {
            var genericType = typeof(DynamoDBDataProvider<>);
            var typedType = genericType.MakeGenericType(type);

            lock (ProviderCache)
            {
                if (!ProviderCache.TryGetValue(type, out var result))
                {
                    result = (IDataProvider)Activator.CreateInstance(typedType);
                    ProviderCache.Add(type, result);
                }

                return result;
            }
        }

        public bool SupportsPolymorphism() => true;
    }
}