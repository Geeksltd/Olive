using Olive.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveEntitiesExtensions
    {
        /// <summary>
        /// This will use Database.Get() to load the specified entity type with this ID.
        /// </summary>
        public static async Task<T> To<T>(this Guid? guid) where T : IEntity
        {
            if (guid == null) return default(T);

            return await guid.Value.To<T>();
        }

        /// <summary>
        /// This will use Database.Get() to load the specified entity type with this ID.
        /// </summary>
        public static async Task<T> To<T>(this Guid guid) where T : IEntity
        {
            if (guid == Guid.Empty) return default(T);

            return await Context.Current.Database().Get<T>(guid);
        }

        public static bool IsAnyOf(this Guid? @this, params GuidEntity[] items)
         => items?.Any(x => x?.ID == @this) ?? false;

        public static bool IsAnyOf(this Guid? @this, IEnumerable<GuidEntity> items)
            => items?.Any(x => x?.ID == @this) ?? false;

        public static bool IsAnyOf(this Guid @this, params GuidEntity[] items)
       => items?.Any(x => x?.ID == @this) ?? false;

        public static bool IsAnyOf(this Guid @this, IEnumerable<GuidEntity> items)
            => items?.Any(x => x?.ID == @this) ?? false;
    }
}