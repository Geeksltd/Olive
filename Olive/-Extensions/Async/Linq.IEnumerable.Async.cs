/* 
 * Linq methods that are:
 *       defined on normal IEnumerable<T> (not Task<IEnumerable<T>>)
 *       but taking a lambda that returns a task
 *       
 * Note: The select methods end with "Async".
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
          this IEnumerable<TSource> @this, Func<TSource, Task<IEnumerable<TResult>>> func)
            => (await @this.SelectAsync(func)).SelectMany(x => x);

        public static Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(
          this IEnumerable<TSource> list, Func<TSource, IEnumerable<Task<TResult>>> func)
            => list.SelectMany(func).AwaitAll();

        public static Task<IEnumerable<T>> Except<T>(this IEnumerable<T> list, Func<T, Task<bool>> criteria)
           => list.Where(i => criteria(i));

        public static async Task<IEnumerable<T>> Where<T>(
          this IEnumerable<T> list, Func<T, Task<bool>> predicate)
        {
            var tasks = list.Select(x => new
            {
                Predicate = predicate(x),
                Value = x
            }).ToArray();

            // Run them in parallel
            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Where(x => x.Predicate.GetAlreadyCompletedResult()).Select(x => x.Value);
        }

        public static async Task<IEnumerable<T>> Concat<T>(
          this IEnumerable<T> list, Task<IEnumerable<T>> second) => list.Concat((await second).OrEmpty());

        public static Task<IEnumerable<T>> Concat<T>(
          this IEnumerable<T> list, IEnumerable<Task<T>> second) => list.Concat(second.AwaitAll());

        public static async Task<IEnumerable<TSource>> Distinct<TSource, TResult>(
          this IEnumerable<TSource> list, Func<TSource, Task<TResult>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Distinct(x => x.Predicate.GetAlreadyCompletedResult()).Select(x => x.Value);
        }

        public static async Task<T> First<T>(
          this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.First(x => x.Predicate.GetAlreadyCompletedResult()).Value;
        }

        public static async Task<T> FirstOrDefault<T>(this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            var result = tasks.FirstOrDefault(x => x.Predicate.GetAlreadyCompletedResult());

            if (result is null) return default(T);
            return result.Value;
        }

        public static async Task<IEnumerable<T>> Intersect<T>(
        this IEnumerable<T> list, Task<IEnumerable<T>> second) => list.Intersect((await second).OrEmpty());

        public static Task<IEnumerable<T>> Intersect<T>(
        this IEnumerable<T> list, IEnumerable<Task<T>> second) => list.Intersect(second.AwaitAll());

        public static async Task<TSource> Last<TSource>(
        this IEnumerable<TSource> list, Func<TSource, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Last(x => x.Predicate.GetAlreadyCompletedResult()).Value;
        }

        public static async Task<T> LastOrDefault<T>(
        this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.LastOrDefault(x => x.Predicate.GetAlreadyCompletedResult()).Value;
        }

        public static async Task<IEnumerable<TSource>> OrderBy<TSource, TKey>(
        this IEnumerable<TSource> list, Func<TSource, Task<TKey>> func)
        {
            var tasks = list.Select(x => new
            {
                OrderKey = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.OrderKey).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.OrderBy(x => x.OrderKey.GetAlreadyCompletedResult()).Select(x => x.Value);
        }

        public static async Task<IEnumerable<TSource>> OrderByDescending<TSource, TKey>(
        this IEnumerable<TSource> list, Func<TSource, Task<TKey>> func)
        {
            var tasks = list.Select(x => new
            {
                OrderKey = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.OrderKey).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.OrderByDescending(x => x.OrderKey.GetAlreadyCompletedResult()).Select(x => x.Value);
        }

        public static async Task<T> Single<T>(this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Single(x => x.Predicate.GetAlreadyCompletedResult()).Value;
        }

        public static async Task<T> SingleOrDefault<T>(this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.SingleOrDefault(x => x.Predicate.GetAlreadyCompletedResult()).Value;
        }

        public static async Task<IEnumerable<T>> Union<T>(this IEnumerable<T> list, Task<IEnumerable<T>> second)
            => list.Union((await second).OrEmpty());

        public static Task<IEnumerable<T>> Union<T>(this IEnumerable<T> list, IEnumerable<Task<T>> second)
            => list.Union(second.AwaitAll());

        public static async Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(this IEnumerable<TSource> list, Task<IEnumerable<TSecond>> second, Func<TSource, TSecond, TResult> func) => list.Zip((await second).OrEmpty(), func);

        public static Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(this IEnumerable<TSource> list, IEnumerable<Task<TSecond>> second, Func<TSource, TSecond, TResult> func) => list.Zip(second.AwaitAll(), func);

        public static Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(this IEnumerable<TSource> list, Task<IEnumerable<TSecond>> second, Func<TSource, TSecond, Task<TResult>> func)
        {
            return list.Zip(second, func);
        }

        public static async Task<IEnumerable<T>> SkipWhile<T>(this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.SkipWhile(x => x.Predicate.GetAlreadyCompletedResult()).Select(x => x.Value);
        }

        public static async Task<IEnumerable<T>> TakeWhile<T>(
        this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.TakeWhile(x => x.Predicate.GetAlreadyCompletedResult()).Select(x => x.Value);
        }

        public static async Task<bool> All<T>(this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.All(x => x.Predicate.GetAlreadyCompletedResult());
        }

        public static async Task<bool> Any<T>(this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Any(x => x.Predicate.GetAlreadyCompletedResult());
        }

        public static async Task<bool> None<T>(this IEnumerable<T> list, Func<T, Task<bool>> func)
            => !await list.Any(func);

        public static async Task<decimal> Average<T>(this IEnumerable<T> list, Func<T, Task<decimal>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Average(x => x.Predicate.GetAlreadyCompletedResult());
        }

        public static async Task<int> Count<T>(this IEnumerable<T> list, Func<T, Task<bool>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Count(x => x.Predicate.GetAlreadyCompletedResult());
        }

        public static async Task<decimal> Sum<T>(this IEnumerable<T> list, Func<T, Task<decimal>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Sum(x => x.Predicate.GetAlreadyCompletedResult());
        }

        public static async Task<TResult> Max<TSource, TResult>(this IEnumerable<TSource> list, Func<TSource, Task<TResult>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }

        public static async Task<TResult> Min<TSource, TResult>(
        this IEnumerable<TSource> list, Func<TSource, Task<TResult>> func)
        {
            var tasks = list.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitAll(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }

        public static async Task<bool> Contains<TSource>(this IEnumerable<TSource> list, Task<TSource> item)
            => list.Contains(await item);
    }
}