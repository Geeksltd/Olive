using System;
using System.Collections.Generic;

namespace Olive.Entities
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SoftDeleteAttribute : Attribute
    {
        static object DetectAndCacheShouldBeStaticMethod = new object();

        static Dictionary<Type, bool> Cache = new Dictionary<Type, bool>();

        /// <summary>
        /// Determines if soft delete is enabled for a given type.
        /// </summary>
        public static bool IsEnabled(Type type) => IsEnabled(type, inherit: true);

        /// <summary>
        /// Determines if soft delete is enabled for a given type.
        /// </summary>
        public static bool IsEnabled(Type type, bool inherit)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (Cache.TryGetValue(type, out var result)) return result;

            return DetectAndCache(type, inherit);
        }

        static bool DetectAndCache(Type type, bool inherit)
        {
            lock (DetectAndCacheShouldBeStaticMethod)
            {
                var result = type.IsDefined(typeof(SoftDeleteAttribute), inherit);

                try { return Cache[type] = result; }
                catch
                {
                    // No logging is needed
                    return result;
                }
            }
        }

        public static bool RequiresSoftdeleteQuery<T>() => RequiresSoftdeleteQuery(typeof(T));

        public static bool RequiresSoftdeleteQuery(Type type)
        {
            if (!IsEnabled(type)) return false;

            return !Context.ShouldByPassSoftDelete();
        }

        /// <summary>
        /// Marks a specified object as soft deleted. 
        /// </summary>
        public static void MarkDeleted(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.IsMarkedSoftDeleted = true;
        }

        /// <summary>
        /// Unmarks a specified object as soft deleted. 
        /// </summary>
        public static void UnMark(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.IsMarkedSoftDeleted = false;
        }

        /// <summary>
        /// Determines if a specified object is marked as soft deleted. 
        /// </summary>
        public static bool IsMarked(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return entity.IsMarkedSoftDeleted;
        }

        /// <summary>
        /// Provides support for bypassing softdelete rule.
        /// </summary>
        public class Context : IDisposable
        {
            bool BypassSoftdelete;

            Context ParentContext;

            public static Context Current
            {
                get => CallContext<Context>.GetData(nameof(Current));
                set => CallContext<Context>.SetData(nameof(Current), value);
            }

            /// <summary>
            /// Creates a new Context instance.
            /// </summary>
            public Context(bool bypassSoftdelete)
            {
                BypassSoftdelete = bypassSoftdelete;

                // Get from current thread:

                if (Current != null)
                    ParentContext = Current;
                Current = this;
            }

            public void Dispose() => Current = ParentContext;

            /// <summary>
            /// Determines if SoftDelete check should the bypassed in the current context.
            /// </summary>
            public static bool ShouldByPassSoftDelete()
            {
                if (Current == null) return false;
                else return Current.BypassSoftdelete;
            }
        }
    }
}