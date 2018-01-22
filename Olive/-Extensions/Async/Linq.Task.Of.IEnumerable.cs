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

        public static async Task<IEnumerable<TSource>> Except<TSource>(
          this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).Except(func);

        public static async Task<IEnumerable<TResult>> Cast<TSource, TResult>(this Task<IEnumerable<TSource>> list)
            => (await list).Cast<TResult>();

        public static async Task<IEnumerable<TSource>> Concat<TSource>(
          this Task<IEnumerable<TSource>> list, IEnumerable<TSource> second)
            => (await list).Concat(second);

        public static async Task<IEnumerable<TSource>> Distinct<TSource, TResult>(
          this Task<IEnumerable<TSource>> list, Func<TSource, TResult> func)
            => (await list).Distinct(func);

        public static async Task<TSource> First<TSource>(
          this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).First(func);

        public static async Task<TSource> FirstOrDefault<TSource>(
          this Task<IEnumerable<TSource>> list, Func<TSource, bool> func)
            => (await list).FirstOrDefault(func);

        public static async Task<IEnumerable<TSource>> Intersect<TSource>(
        this Task<IEnumerable<TSource>> list, IEnumerable<TSource> second) => (await list).Intersect(second);

        public static async Task<TSource> Last<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func) => (await list).Last(func);

        public static async Task<TSource> LastOrDefault<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func) => (await list).LastOrDefault(func);

        public static async Task<IOrderedEnumerable<TSource>> OrderBy<TSource, TKey>(
        this Task<IEnumerable<TSource>> list, Func<TSource, TKey> func) => (await list).OrderBy(func);

        public static async Task<IOrderedEnumerable<TSource>> OrderByDescending<TSource, TKey>(
        this Task<IEnumerable<TSource>> list, Func<TSource, TKey> func) => (await list).OrderByDescending(func);

        public static async Task<IEnumerable<TSource>> Reverse<TSource>(
        this Task<IEnumerable<TSource>> list) => (await list).Reverse();

        public static async Task<TSource> Single<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func) => (await list).Single(func);

        public static async Task<TSource> SingleOrDefault<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func) => (await list).SingleOrDefault(func);

        public static async Task<IEnumerable<TSource>> Union<TSource>(
        this Task<IEnumerable<TSource>> list, IEnumerable<TSource> second) => (await list).Union(second);

        public static async Task<IEnumerable<TSource>> Where<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func) => (await list).Where(func);

        public static async Task<IEnumerable<TSource>> Where<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, int, bool> func) => (await list).Where(func);

        public static async Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(
        this Task<IEnumerable<TSource>> list, IEnumerable<TSecond> second, Func<TSource, TSecond, TResult> func) => (await list).Zip(second, func);

        public static async Task<IEnumerable<TSource>> Skip<TSource>(
        this Task<IEnumerable<TSource>> list, int count) => (await list).Skip(count);

        public static async Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func) => (await list).SkipWhile(func);

        public static async Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, int, bool> func) => (await list).SkipWhile(func);

        public static async Task<IEnumerable<TSource>> Take<TSource>(
        this Task<IEnumerable<TSource>> list, int count) => (await list).Take(count);

        public static async Task<IEnumerable<TSource>> Take<TSource>(
        this Task<IEnumerable<TSource>> list, int lower, int upper) => (await list).Take(lower, upper);

        public static async Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func) => (await list).TakeWhile(func);

        public static async Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, int, bool> func) => (await list).TakeWhile(func);

        public static async Task<TSource> Aggregate<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, TSource, TSource> func) => (await list).Aggregate(func);

        public static async Task<TAccumulate> Aggregate<TSource, TAccumulate>(
        this Task<IEnumerable<TSource>> list, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func) => (await list).Aggregate(seed, func);

        public static async Task<TResult> Aggregate<TSource, TAccumulate, TResult>(
        this Task<IEnumerable<TSource>> list, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func1, Func<TAccumulate, TResult> func2) => (await list).Aggregate(seed, func1, func2);

        public static async Task<bool> All<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func) => (await list).All(func);

        public static async Task<bool> Any<TSource>(
        this Task<IEnumerable<TSource>> list) => (await list).Any();

        public static async Task<bool> Any<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func) => (await list).Any(func);

        public static async Task<bool> Any<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, int, bool> func) => (await list).Any(func);

        public static async Task<decimal> Average<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, decimal> func) => (await list).Average(func);

        public static async Task<int> Count<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, bool> func) => (await list).Count(func);

        public static async Task<int> Count<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, int, bool> func) => (await list).Count(func);

        public static async Task<decimal> Sum<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, decimal> func) => (await list).Sum(func);

        public static async Task<TimeSpan> Sum<TSource>(
        this Task<IEnumerable<TSource>> list, Func<TSource, TimeSpan> func) => (await list).Sum(func);

        public static async Task<TResult> Max<TSource, TResult>(
        this Task<IEnumerable<TSource>> list, Func<TSource, TResult> func) => (await list).Max(func);

        public static async Task<TResult> Min<TSource, TResult>(
        this Task<IEnumerable<TSource>> list, Func<TSource, TResult> func) => (await list).Min(func);

        public static async Task<IEnumerable<TResult>> Cast<TResult>(this Task<IEnumerable> list)
            => (await list).Cast<TResult>();

        public static async Task<IEnumerable<TResult>> OfType<TResult>(this Task<IEnumerable> list)
            => (await list).OfType<TResult>();
    }
}