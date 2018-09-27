using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.Entities.Data
{
    public static class CacheExtensions
    {
        public static bool IsCacheable(this Type type) => CacheObjectsAttribute.IsEnabled(type) ?? Database.Configuration.Cache.Enabled;
    }
}
