namespace Olive.Entities.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    partial class DatabaseQuery
    {
        bool NeedsTypeResolution() => EntityType.IsInterface || EntityType == typeof(Entity);
    }
}