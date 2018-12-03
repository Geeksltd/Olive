using Olive.Entities;
using System;
using System.Collections.Generic;
using CustomEntityBinder = System.Func<string, System.Threading.Tasks.Task<Olive.Entities.IEntity>>;

namespace Olive.Mvc
{
    partial class EntityModelBinder
    {
        CustomEntityBinder CustomBinder;
        static Dictionary<Type, CustomEntityBinder> CustomParsers = new Dictionary<Type, CustomEntityBinder>();

        /// <summary>
        /// Will register a custom binder for a type instead of the default which uses a Database.Get.
        /// </summary>
        public static void RegisterCustomParser<TEntity>(CustomEntityBinder binder) where TEntity : IEntity
        {
            CustomParsers.Add(typeof(TEntity), binder);
        }

        static CustomEntityBinder FindCustomParser(Type entityType)
        {
            foreach (var actualType in entityType.WithAllParents())
                if (CustomParsers.TryGetValue(actualType, out var result))
                    return result;

            return null;
        }
    }
}