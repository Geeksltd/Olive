/* 
 * Linq methods that are:
 *       defined on normal IEnumerable<T> (not Task<IEnumerable<T>>)
 *       but taking a lambda that returns a task
 *       
 * Note: These methods end with "Async".
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        public static Task<List<T>> ToListAsync<T>(this IEnumerable<Task<T>> list)
           => list.AwaitAll().ToList();

        public static Task<T[]> ToArrayAsync<T>(this IEnumerable<Task<T>> list)
           => list.AwaitAll().ToArray();

        public static Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
          this IEnumerable<TSource> list, Func<TSource, Task<TResult>> func)
            => list.Select(func).AwaitAll();

        public static async Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(
          this IEnumerable<TSource> list, Func<TSource, Task<IEnumerable<TResult>>> func)
            => (await list.SelectAsync(func)).SelectMany(x => x);

        public static Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(
          this IEnumerable<TSource> list, Func<TSource, IEnumerable<Task<TResult>>> func)
            => list.SelectMany(func).AwaitAll();

        public static Task<IEnumerable<T>> ExceptAsync<T>(this IEnumerable<T> list, Func<T, Task<bool>> criteria)
           => list.WhereAsync(i => criteria(i));

        public static async Task<IEnumerable<T>> WhereAsync<T>(
          this IEnumerable<T> list, Func<T, Task<bool>> predicate)
        {
            var tasks = list.Select(x => new
            {
                Predicate = predicate(x),
                Value = x
            }).ToArray();

            // Run them in parallel
            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.Where(x => !x.Predicate.Result).Select(x => x.Value);
        }
    }
}