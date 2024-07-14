using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Olive
{
    partial class Bindable<TValue>
    {
        readonly ConcurrentDictionary<string, IBindable> Dependents = new();

        /// <summary>
        /// Returns a durable unique nested Bindable whose value remains in sync with this instance.
        /// </summary>
        public Bindable<TTarget> Get<TTarget>(Func<TValue, TTarget> valueProvider,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            return Get(null, valueProvider, callerFile, callerLine);
        }

        /// <summary>
        /// Returns a durable unique nested Bindable whose value remains in sync with this instance.
        /// </summary>
        public Bindable<TTarget> Get<TTarget>(string uniqueIdentifier, Func<TValue, TTarget> valueProvider,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            return GetDependent(uniqueIdentifier, _ =>
            {
                var result = new ExpressionBindable<TTarget, TValue>(this, valueProvider);
                result.Bind(this, valueProvider);
                return result;
            }, callerFile, callerLine);
        }

        /// <summary>
        /// Returns a durable unique nested Bindable whose value remains in sync with this instance.
        /// </summary>
        public TTarget GetDependent<TTarget>(string uniqueIdentifier, Func<string, TTarget> valueProvider,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0) where TTarget : IBindable
        {
            var key = $"{callerFile}|{callerLine}|{uniqueIdentifier}";
            return (TTarget)Dependents.GetOrAdd(key, x => valueProvider(x));
        }
    }
}