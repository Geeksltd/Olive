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

            return tasks.Where(x => x.Predicate.Result).Select(x => x.Value);
        }

        public static async Task<IEnumerable<T>> ConcatAsync<T>(
          this IEnumerable<T> list, Task<IEnumerable<T>> second) => list.Concat(await second);

        public static Task<IEnumerable<T>> ConcatAsync<T>(
          this IEnumerable<T> list, IEnumerable<Task<T>> second) => list.ConcatAsync(second.AwaitAll());

        public static async Task<IEnumerable<TSource>> DistinctAsync<TSource, TResult>(
          this IEnumerable<TSource> list, Func<TSource, Task<TResult>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.Distinct(x => x.Predicate.Result).Select(x => x.Value);
        }

        public static async Task<T> FirstAsync<T>(
          this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.First(x => x.Predicate.Result).Value;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(
          this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.FirstOrDefault(x => x.Predicate.Result).Value;
        }

        public static async Task<IEnumerable<T>> IntersectAsync<T>(
        this IEnumerable<T> list, Task<IEnumerable<T>> second) => list.Intersect(await second);

        public static Task<IEnumerable<T>> IntersectAsync<T>(
        this IEnumerable<T> list, IEnumerable<Task<T>> second) => list.IntersectAsync(second.AwaitAll());

        public static async Task<TSource> LastAsync<TSource>(
        this IEnumerable<TSource> list, Func<TSource, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.Last(x => x.Predicate.Result).Value;
        }

        public static async Task<T> LastOrDefaultAsync<T>(
        this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.LastOrDefault(x => x.Predicate.Result).Value;
        }

        public static async Task<IEnumerable<TSource>> OrderByAsync<TSource, TKey>(
        this IEnumerable<TSource> list, Func<TSource, Task<TKey>> func)
        {
            var tasks = list.Select(x => new
            {
                OrderKey = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.OrderKey).ConfigureAwait(false);

            return tasks.OrderBy(x => x.OrderKey.Result).Select(x => x.Value);
        }

        public static async Task<IEnumerable<TSource>> OrderByDescendingAsync<TSource, TKey>(
        this IEnumerable<TSource> list, Func<TSource, Task<TKey>> func)
        {
            var tasks = list.Select(x => new
            {
                OrderKey = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.OrderKey).ConfigureAwait(false);

            return tasks.OrderByDescending(x => x.OrderKey.Result).Select(x => x.Value);
        }

        public static async Task<T> SingleAsync<T>(this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.Single(x => x.Predicate.Result).Value;
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.SingleOrDefault(x => x.Predicate.Result).Value;
        }

        public static async Task<IEnumerable<T>> UnionAsync<T>(this IEnumerable<T> list, Task<IEnumerable<T>> second) => list.Union(await second);

        public static Task<IEnumerable<T>> UnionAsync<T>(this IEnumerable<T> list, IEnumerable<Task<T>> second) => list.UnionAsync(second.AwaitAll());

        public static async Task<IEnumerable<TResult>> ZipAsync<TSource, TSecond, TResult>(this IEnumerable<TSource> list, Task<IEnumerable<TSecond>> second, Func<TSource, TSecond, TResult> func) => list.Zip(await second, func);

        public static Task<IEnumerable<TResult>> ZipAsync<TSource, TSecond, TResult>(this IEnumerable<TSource> list, IEnumerable<Task<TSecond>> second, Func<TSource, TSecond, TResult> func) => list.ZipAsync(second.AwaitAll(), func);

        public static Task<IEnumerable<TResult>> ZipAsync<TSource, TSecond, TResult>(this IEnumerable<TSource> list, Task<IEnumerable<TSecond>> second, Func<TSource, TSecond, Task<TResult>> func)
        {
            return list.ZipAsync(second, func);
        }

        public static async Task<IEnumerable<T>> SkipWhileAsync<T>(
        this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.SkipWhile(x => x.Predicate.Result).Select(x => x.Value);
        }

        public static async Task<IEnumerable<T>> TakeWhileAsync<T>(
        this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.TakeWhile(x => x.Predicate.Result).Select(x => x.Value);
        }

        public static async Task<bool> AllAsync<T>(this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.All(x => x.Predicate.Result);
        }

        public static async Task<bool> AnyAsync<T>(this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.Any(x => x.Predicate.Result);
        }

        public static async Task<decimal> AverageAsync<T>(this IEnumerable<T> list, Func<T, Task<decimal>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.Average(x => x.Predicate.Result);
        }

        public static async Task<int> CountAsync<T>(this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.Count(x => x.Predicate.Result);
        }

        public static async Task<decimal> SumAsync<T>(this IEnumerable<T> list, Func<T, Task<decimal>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.Sum(x => x.Predicate.Result);
        }

        public static async Task<TResult> MaxAsync<TSource, TResult>(this IEnumerable<TSource> list, Func<TSource, Task<TResult>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.Max(x => x.Predicate.Result);
        }

        public static async Task<TResult> MinAsync<TSource, TResult>(
        this IEnumerable<TSource> list, Func<TSource, Task<TResult>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(false);

            return tasks.Min(x => x.Predicate.Result);
        }
    }
}