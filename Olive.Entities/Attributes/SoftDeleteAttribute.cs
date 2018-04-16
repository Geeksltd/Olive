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
        public static bool IsEnabled(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (Cache.TryGetValue(type, out var result)) return result;

            return DetectAndCache(type);
        }

        static bool DetectAndCache(Type type)
        {
            lock (DetectAndCacheShouldBeStaticMethod)
            {
                var result = type.IsDefined(typeof(SoftDeleteAttribute), inherit: true);

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