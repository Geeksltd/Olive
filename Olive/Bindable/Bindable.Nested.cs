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
            var key = $"{callerFile}|{callerLine}|{uniqueIdentifier}";

            return (Bindable<TTarget>)Dependents.GetOrAdd(key, k =>
            {
                var result = new ExpressionBindable<TTarget, TValue>(this, valueProvider);
                result.Bind(this, valueProvider);
                return result;
            });
        }
    }
}