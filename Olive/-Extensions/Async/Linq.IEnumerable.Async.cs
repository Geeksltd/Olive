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
        public static Task<List<T>> ToListAsync<T>(this IEnumerable<Task<T>> @this)
           => @this.AwaitAll().ToList();

        public static Task<T[]> ToArrayAsync<T>(this IEnumerable<Task<T>> @this)
           => @this.AwaitAll().ToArray();

        public static Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(
          this IEnumerable<TSource> @this, Func<TSource, Task<TResult>> func)
            => @this.Select(func).AwaitAll();

        public static Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(
          this IEnumerable<TSource> @this, Func<TSource, Task<IEnumerable<TResult>>> func)
            => @this.SelectAsync(func).Get(v => v.SelectMany(x => x));

        public static Task<IEnumerable<TResult>> SelectManyAsync<TSource, TResult>(
          this IEnumerable<TSource> @this, Func<TSource, IEnumerable<Task<TResult>>> func)
            => @this.SelectMany(func).AwaitAll();

        public static Task<IEnumerable<T>> Except<T>(this IEnumerable<T> @this, Func<T, Task<bool>> criteria)
           => @this.Where(i => criteria(i));

        public static async Task<IEnumerable<T>> Where<T>(
          this IEnumerable<T> @this, Func<T, Task<bool>> predicate)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = predicate(x),
                Value = x
            }).ToArray();

            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Where(x => x.Predicate.GetAlreadyCompletedResult()).Select(x => x.Value);
        }

        public static async Task<IEnumerable<T>> Concat<T>(
          this IEnumerable<T> @this, Task<IEnumerable<T>> second) => @this.Concat((await second).OrEmpty());

        public static Task<IEnumerable<T>> Concat<T>(
          this IEnumerable<T> @this, IEnumerable<Task<T>> second) => @this.Concat(second.AwaitAll());

        public static async Task<IEnumerable<TSource>> Distinct<TSource, TResult>(
          this IEnumerable<TSource> @this, Func<TSource, Task<TResult>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Distinct(x => x.Predicate.GetAlreadyCompletedResult()).Select(x => x.Value);
        }

        public static async Task<T> First<T>(
          this IEnumerable<T> @this, Func<T, Task<bool>> func)
        {
            foreach (var item in @this)
                if (await func(item)) return item;

            throw new InvalidOperationException("No item in the source sequence matches the provided predicte.");
        }

        public static async Task<T> FirstOrDefault<T>(this IEnumerable<T> @this, Func<T, Task<bool>> func)
        {
            foreach (var item in @this)
                if (await func(item)) return item;

            return default(T);
        }

        public static async Task<IEnumerable<T>> Intersect<T>(
        this IEnumerable<T> @this, Task<IEnumerable<T>> second) => @this.Intersect((await second).OrEmpty());

        public static Task<IEnumerable<T>> Intersect<T>(
        this IEnumerable<T> @this, IEnumerable<Task<T>> second) => @this.Intersect(second.AwaitAll());

        public static Task<TSource> Last<TSource>(this IEnumerable<TSource> @this, Func<TSource, Task<bool>> func)
        {
            return @this.Reverse().First(func);
        }

        public static Task<T> LastOrDefault<T>(this IEnumerable<T> @this, Func<T, Task<bool>> func)
        {
            return @this.Reverse().FirstOrDefault(func);
        }

        public static async Task<IEnumerable<TSource>> OrderBy<TSource, TKey>(
        this IEnumerable<TSource> @this, Func<TSource, Task<TKey>> func)
        {
            var tasks = @this.Select(x => new
            {
                OrderKey = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitSequential(x => x.OrderKey).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.OrderBy(x => x.OrderKey.GetAlreadyCompletedResult()).Select(x => x.Value);
        }

        public static async Task<IEnumerable<TSource>> OrderByDescending<TSource, TKey>(
        this IEnumerable<TSource> @this, Func<TSource, Task<TKey>> func)
        {
            var tasks = @this.Select(x => new { OrderKey = func(x), Value = x }).ToArray();

            await tasks.AwaitSequential(x => x.OrderKey).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.OrderByDescending(x => x.OrderKey.GetAlreadyCompletedResult()).Select(x => x.Value);
        }

        public static async Task<T> Single<T>(this IEnumerable<T> @this, Func<T, Task<bool>> func)
        {
            var tasks = @this.Select(x => new { Predicate = func(x), Value = x }).ToArray();

            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Single(x => x.Predicate.GetAlreadyCompletedResult()).Value;
        }

        public static async Task<T> SingleOrDefault<T>(this IEnumerable<T> @this, Func<T, Task<bool>> func)
        {
            var tasks = @this.Select(x => new { Predicate = func(x), Value = x }).ToArray();

            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.SingleOrDefault(x => x.Predicate.GetAlreadyCompletedResult()).Value;
        }

        public static async Task<IEnumerable<T>> Union<T>(this IEnumerable<T> @this, Task<IEnumerable<T>> second)
            => @this.Union((await second).OrEmpty());

        public static Task<IEnumerable<T>> Union<T>(this IEnumerable<T> @this, IEnumerable<Task<T>> second)
            => @this.Union(second.AwaitAll());

        public static async Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(this IEnumerable<TSource> @this, Task<IEnumerable<TSecond>> second, Func<TSource, TSecond, TResult> func) => @this.Zip((await second).OrEmpty(), func);

        public static Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(this IEnumerable<TSource> @this, IEnumerable<Task<TSecond>> second, Func<TSource, TSecond, TResult> func) => @this.Zip(second.AwaitAll(), func);

        public static Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(this IEnumerable<TSource> @this, Task<IEnumerable<TSecond>> second, Func<TSource, TSecond, Task<TResult>> func)
        {
            return @this.Zip(second, func);
        }

        public static async Task<IEnumerable<T>> SkipWhile<T>(this IEnumerable<T> @this, Func<T, Task<bool>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.SkipWhile(x => x.Predicate.GetAlreadyCompletedResult()).Select(x => x.Value);
        }

        public static async Task<IEnumerable<T>> TakeWhile<T>(this IEnumerable<T> @this, Func<T, Task<bool>> func)
        {
            var result = new List<T>();

            foreach (var item in @this)
            {
                if (await func(item).ConfigureAwait(false)) result.Add(item);
                else break;
            }

            return result;
        }

        public static async Task<bool> All<T>(this IEnumerable<T> @this, Func<T, Task<bool>> func)
        {
            foreach (var item in @this)
                if (!await func(item).ConfigureAwait(false))
                    return false;

            return true;
        }

        public static async Task<bool> Any<T>(this IEnumerable<T> @this, Func<T, Task<bool>> func)
        {
            foreach (var item in @this)
                if (await func(item).ConfigureAwait(false))
                    return true;

            return false;
        }

        public static async Task<bool> None<T>(this IEnumerable<T> @this, Func<T, Task<bool>> func)
            => !await @this.Any(func).ConfigureAwait(false);

        public static async Task<int> Count<T>(this IEnumerable<T> @this, Func<T, Task<bool>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Count(x => x.Predicate.GetAlreadyCompletedResult());
        }

        public static async Task<TResult> Max<TSource, TResult>(this IEnumerable<TSource> @this, Func<TSource, Task<TResult>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Max(x => x.Predicate.GetAlreadyCompletedResult());
        }

        public static async Task<TResult> Min<TSource, TResult>(
        this IEnumerable<TSource> @this, Func<TSource, Task<TResult>> func)
        {
            var tasks = @this.Select(x => new
            {
                Predicate = func(x),
                Value = x
            }).ToArray();

            await tasks.AwaitSequential(x => x.Predicate).ConfigureAwait(continueOnCapturedContext: false);

            return tasks.Min(x => x.Predicate.GetAlreadyCompletedResult());
        }

        public static async Task<bool> Contains<TSource>(this IEnumerable<TSource> @this, Task<TSource> item)
            => @this.Contains(await item.ConfigureAwait(false));

        /// <summary>
        /// If a specified condition is true, then the filter predicate will be executed.
        /// Otherwise the original list will be returned.
        /// </summary>
        [EscapeGCop("The condition param should not be last in this case.")]
        public static Task<IEnumerable<T>> FilterIf<T>(this IEnumerable<Task<T>> source,
             bool condition, Func<T, bool> predicate)
            => condition ? source.AwaitAll().Where(predicate) : source.AwaitAll();

        /// <summary>
        /// If a specified condition is true, then the filter predicate will be executed.
        /// Otherwise the original list will be returned.
        /// </summary>
        [EscapeGCop("The condition param should not be last in this case.")]
        public static async Task<IEnumerable<T>> FilterIf<T>(this IEnumerable<T> source,
             bool condition, Func<T, Task<bool>> predicate)
            => condition ? await source.Where(predicate) : source;
    }
}