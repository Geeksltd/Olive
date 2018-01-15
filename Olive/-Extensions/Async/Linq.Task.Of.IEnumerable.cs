/* 
 * Linq methods that are:
 *       defined on Task<IEnumerable<T>>
 *       and take lambdas that either return the result or a task of result
 *       
 * Note: These methods do not end with "Async".
 * They are intended to enable chaining of async linq calls.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        public static async Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, TResult> func)
            => (await list).Select(func);

        public static async Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, Task<TResult>> func)
            => await (await list).Select(func).AwaitAll();

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, IEnumerable<TResult>> func)
            => (await list).SelectMany(func);

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, IEnumerable<Task<TResult>>> func)
            => await (await list).SelectMany(func).AwaitAll();

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, Task<IEnumerable<TResult>>> func)
            => await (await list).Select(func).AwaitAll().SelectMany(x => x);

        public static async Task<IEnumerable<TSource>> Except<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).Except(func);

        public static async Task<IEnumerable<TResult>> Cast<TSource, TResult>(this Task<IEnumerable<TSource>> list)
            => (await list).Cast<TResult>();

        public static async Task<IEnumerable<TSource>> Concat<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, IEnumerable<TSource> second)
            => (await list).Concat(second);

        public static async Task<IEnumerable<TSource>> Distinct<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, TResult> func)
            => (await list).Distinct(func);

        public static async Task<TSource> First<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).First(func);

        public static async Task<TSource> FirstOrDefault<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).FirstOrDefault(func);
    }
}