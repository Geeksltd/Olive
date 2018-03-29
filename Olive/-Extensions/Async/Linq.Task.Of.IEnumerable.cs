/* 
 * Linq methods that are:
 *       defined on Task<IEnumerable<T>>
 *       and take lambdas that either return the result or a task of result
 *       
 * Note: These methods do not end with "Async".
 * They are intended to enable chaining of async linq calls.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Olive
{
    partial class OliveExtensions
    {
        public static async Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, TResult> func)
            => (await list).OrEmpty().Select(func);

        public static async Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, Task<TResult>> func)
            => await (await list).OrEmpty().Select(func).AwaitAll();

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, IEnumerable<TResult>> func)
            => (await list).OrEmpty().SelectMany(func);

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, IEnumerable<Task<TResult>>> func)
            => await (await list).OrEmpty().SelectMany(func).AwaitAll();

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, Task<IEnumerable<TResult>>> func)
            => await (await list).OrEmpty().Select(func).AwaitAll().SelectMany(x => x);

        public static async Task<IEnumerable<TSource>> Except<TSource>(
          this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().Except(func);

        public static Task<IEnumerable<TSource>> ExceptNull<TSource>(
         this Task<IEnumerable<TSource>> list) => list.Where(x => !ReferenceEquals(x, null));

        public static async Task<IEnumerable<TResult>> Cast<TSource, TResult>(this Task<IEnumerable<TSource>> list)
            => (await list).OrEmpty().Cast<TResult>();

        public static async Task<IEnumerable<TSource>> Concat<TSource>(
          this Task<IEnumerable<TSource>> list, IEnumerable<TSource> second)
            => (await list).OrEmpty().Concat(second);

        public static async Task<IEnumerable<TSource>> Distinct<TSource>(
          this Task<IEnumerable<TSource>> list) => (await list).OrEmpty().Distinct();

        public static async Task<IEnumerable<TSource>> Distinct<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, TResult> func)
            => (await list).OrEmpty().Distinct(func);

        public static async Task<TSource> First<TSource>(
          this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().First(func);

        public static async Task<TSource> FirstOrDefault<TSource>(
          this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().FirstOrDefault(func);

        public static async Task<IEnumerable<TSource>> Intersect<TSource>(
        this Task<IEnumerable<TSource>> list, IEnumerable<TSource> second)
            => (await list).OrEmpty().Intersect(second);

        public static async Task<TSource> Last<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().Last(func);

        public static async Task<TSource> LastOrDefault<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().LastOrDefault(func);

        public static async Task<IOrderedEnumerable<TSource>> OrderBy<TSource, TKey>(
        this Task<IEnumerable<TSource>> list, Func<TSource, TKey> func)
            => (await list).OrEmpty().OrderBy(func);

        public static async Task<IOrderedEnumerable<TSource>> OrderByDescending<TSource, TKey>(
        this Task<IEnumerable<TSource>> list, Func<TSource, TKey> func)
            => (await list).OrEmpty().OrderByDescending(func);

        public static async Task<IEnumerable<TSource>> Reverse<TSource>(
        this Task<IEnumerable<TSource>> list) => (await list).OrEmpty().Reverse();

        public static async Task<TSource> Single<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().Single(func);

        public static async Task<TSource> SingleOrDefault<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().SingleOrDefault(func);

        public static async Task<IEnumerable<TSource>> Union<TSource>(
        this Task<IEnumerable<TSource>> list, IEnumerable<TSource> second)
            => (await list).OrEmpty().Union(second);

        public static async Task<IEnumerable<TSource>> Where<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().Where(func);

        public static async Task<IEnumerable<TSource>> Where<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, int, bool> func)
            => (await list).OrEmpty().Where(func);

        public static async Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(
        this Task<IEnumerable<TSource>> list, IEnumerable<TSecond> second, Func<TSource, TSecond, TResult> func)
            => (await list).OrEmpty().Zip(second, func);

        public static async Task<IEnumerable<TSource>> Skip<TSource>(
        this Task<IEnumerable<TSource>> list, int count) => (await list).OrEmpty().Skip(count);

        public static async Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().SkipWhile(func);

        public static async Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, int, bool> func)
            => (await list).OrEmpty().SkipWhile(func);

        public static async Task<IEnumerable<TSource>> Take<TSource>(
        this Task<IEnumerable<TSource>> list, int count)
            => (await list).OrEmpty().Take(count);

        public static async Task<IEnumerable<TSource>> Take<TSource>(
        this Task<IEnumerable<TSource>> list, int lower, int count)
            => (await list).OrEmpty().Take(lower, count);

        public static async Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().TakeWhile(func);

        public static async Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, int, bool> func)
            => (await list).OrEmpty().TakeWhile(func);

        public static async Task<TSource> Aggregate<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, TSource, TSource> func)
            => (await list).OrEmpty().Aggregate(func);

        public static async Task<TAccumulate> Aggregate<TSource, TAccumulate>(
        this Task<IEnumerable<TSource>> list, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => (await list).OrEmpty().Aggregate(seed, func);

        public static async Task<TResult> Aggregate<TSource, TAccumulate, TResult>(
        this Task<IEnumerable<TSource>> list, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func1, Func<TAccumulate, TResult> func2) => (await list).OrEmpty().Aggregate(seed, func1, func2);

        public static async Task<bool> All<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().All(func);

        public static async Task<bool> Any<TSource>(this Task<IEnumerable<TSource>> list)
            => (await list).OrEmpty().Any();

        public static async Task<bool> Any<TSource>(this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().Any(func);

        public static async Task<bool> Any<TSource>(this Task<IEnumerable<TSource>> list, Func<TSource, Task<bool>> func)
            => await (await list).OrEmpty().Any(func);

        public static async Task<bool> Any<TSource>(this Task<IEnumerable<TSource>> list, Func<TSource, int, bool> func)
            => (await list).OrEmpty().Any(func);

        public static async Task<bool> None<TSource>(this Task<IEnumerable<TSource>> list)
          => (await list).OrEmpty().None();

        public static async Task<bool> None<TSource>(this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().None(func);

        public static async Task<bool> None<TSource>(this Task<IEnumerable<TSource>> list, Func<TSource, Task<bool>> func)
            => await (await list).OrEmpty().None(func);

        public static async Task<bool> None<TSource>(this Task<IEnumerable<TSource>> list, Func<TSource, int, bool> func)
            => (await list).OrEmpty().None(func);

        public static async Task<bool> Contains<TSource>(this Task<IEnumerable<TSource>> list, TSource item)
            => (await list).OrEmpty().Contains(item);

        public static async Task<bool> Contains<TSource>(this Task<IEnumerable<TSource>> list, Task<TSource> item)
            => (await list).OrEmpty().Contains(await item);

        public static async Task<decimal> Average<TSource>(this Task<IEnumerable<TSource>> list, Func<TSource, decimal> func)
            => (await list).OrEmpty().Average(func);

        public static async Task<int> Count<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).OrEmpty().Count(func);

        public static async Task<int> Count<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, int, bool> func)
            => (await list).OrEmpty().Count(func);

        public static async Task<decimal> Sum<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, decimal> func)
            => (await list).OrEmpty().Sum(func);

        public static async Task<TimeSpan> Sum<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, TimeSpan> func)
            => (await list).OrEmpty().Sum(func);

        public static async Task<TResult> Max<TSource, TResult>(
        this Task<IEnumerable<TSource>> list, Func<TSource, TResult> func)
            => (await list).OrEmpty().Max(func);

        public static async Task<TResult> Min<TSource, TResult>(
        this Task<IEnumerable<TSource>> list, Func<TSource, TResult> func)
            => (await list).OrEmpty().Min(func);

        public static async Task<IEnumerable<TResult>> Cast<TResult>(this Task<IEnumerable> list)
            => (await list).Cast<TResult>();

        public static async Task<IEnumerable<TResult>> OfType<TResult>(this Task<IEnumerable> list)
            => (await list).OfType<TResult>();
    }
}