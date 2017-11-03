using System;
using System.Threading.Tasks;

namespace Olive.Entities
{
    partial class OliveExtensions
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

            return await Entity.Database.Get<T>(guid);
        }
    }
}