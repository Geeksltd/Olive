namespace Olive.Entities.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    partial class DatabaseQuery
    {
        List<IDataProvider> ResolveDataProviders()
        {
            var factories = Context.Current.Database().AssemblyProviderFactories
                .Where(f => f.Value.SupportsPolymorphism())
                .Where(f => f.Key.References(EntityType.GetTypeInfo().Assembly)).ToList();

            var result = new List<IDataProvider>();

            foreach (var f in factories)
                result.Add(f.Value.GetProvider(EntityType));

            foreach (var type in EntityFinder.FindPossibleTypes(EntityType, mustFind: factories.None()))
                result.Add(Context.Current.Database().GetProvider(type));

            return result;
        }

        bool NeedsTypeResolution() => EntityType.IsInterface || EntityType == typeof(Entity);
    }
}