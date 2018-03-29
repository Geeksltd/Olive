/* 
 * Linq methods that are:
 *       defined on Task<T[]>
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
        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<TSource[]> list, Func<TSource, TResult> func)
            => list.ForLinq().Select(func);

        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<TSource[]> list, Func<TSource, Task<TResult>> func)
            => list.ForLinq().Select(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> list, Func<TSource, IEnumerable<TResult>> func)
            => list.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> list, Func<TSource, IEnumerable<Task<TResult>>> func)
            => list.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> list, Func<TSource, Task<IEnumerable<TResult>>> func)
            => list.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TSource>> Except<TSource>(
          this Task<TSource[]> list, Func<TSource, bool> func)
            => list.ForLinq().Except(func);

        public static Task<IEnumerable<TSource>> ExceptNull<TSource>(this Task<TSource[]> list)
           => list.ForLinq().ExceptNull();

        public static Task<IEnumerable<TResult>> Cast<TSource, TResult>(this Task<TSource[]> list)
            => list.ForLinq().Cast<TSource, TResult>();

        public static Task<IEnumerable<TSource>> Concat<TSource>(
          this Task<TSource[]> list, IEnumerable<TSource> second)
            => list.ForLinq().Concat(second);

        public static Task<IEnumerable<TSource>> Distinct<TSource, TResult>(this Task<TSource[]> list, Func<TSource, TResult> func)
            => list.ForLinq().Distinct(func);

        public static Task<IEnumerable<TSource>> Distinct<TSource>(this Task<TSource[]> list)
            => list.ForLinq().Distinct();

        public static Task<TSource> First<TSource>(
          this Task<TSource[]> list, Func<TSource, bool> func)
            => list.ForLinq().First(func);

        public static Task<TSource> FirstOrDefault<TSource>(
          this Task<TSource[]> list, Func<TSource, bool> func)
            => list.ForLinq().FirstOrDefault(func);

        public static Task<IEnumerable<TSource>> Intersect<TSource>(
        this Task<TSource[]> list, IEnumerable<TSource> second) => list.ForLinq().Intersect(second);

        public static Task<TSource> Last<TSource>(
        this Task<TSource[]> list, Func<TSource, bool> func) => list.ForLinq().Last(func);

        public static Task<TSource> LastOrDefault<TSource>(
        this Task<TSource[]> list, Func<TSource, bool> func) => list.ForLinq().LastOrDefault(func);

        public static Task<IOrderedEnumerable<TSource>> OrderBy<TSource, TKey>(
        this Task<TSource[]> list, Func<TSource, TKey> func) => list.ForLinq().OrderBy(func);

        public static Task<IOrderedEnumerable<TSource>> OrderByDescending<TSource, TKey>(
        this Task<TSource[]> list, Func<TSource, TKey> func) => list.ForLinq().OrderByDescending(func);

        public static Task<IEnumerable<TSource>> Reverse<TSource>(
        this Task<TSource[]> list) => list.ForLinq().Reverse();

        public static Task<TSource> Single<TSource>(
        this Task<TSource[]> list, Func<TSource, bool> func) => list.ForLinq().Single(func);

        public static Task<TSource> SingleOrDefault<TSource>(
        this Task<TSource[]> list, Func<TSource, bool> func) => list.ForLinq().SingleOrDefault(func);

        public static Task<IEnumerable<TSource>> Union<TSource>(
        this Task<TSource[]> list, IEnumerable<TSource> second) => list.ForLinq().Union(second);

        public static Task<IEnumerable<TSource>> Where<TSource>(
        this Task<TSource[]> list, Func<TSource, bool> func) => list.ForLinq().Where(func);

        public static Task<IEnumerable<TSource>> Where<TSource>(
        this Task<TSource[]> list, Func<TSource, int, bool> func) => list.ForLinq().Where(func);

        public static Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(
        this Task<TSource[]> list, IEnumerable<TSecond> second, Func<TSource, TSecond, TResult> func)
            => list.ForLinq().Zip(second, func);

        public static Task<IEnumerable<TSource>> Skip<TSource>(
        this Task<TSource[]> list, int count) => list.ForLinq().Skip(count);

        public static Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<TSource[]> list, Func<TSource, bool> func) => list.ForLinq().SkipWhile(func);

        public static Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<TSource[]> list, Func<TSource, int, bool> func) => list.ForLinq().SkipWhile(func);

        public static Task<IEnumerable<TSource>> Take<TSource>(
        this Task<TSource[]> list, int count) => list.ForLinq().Take(count);

        public static Task<IEnumerable<TSource>> Take<TSource>(
        this Task<TSource[]> list, int lower, int count) => list.ForLinq().Take(lower, count);

        public static Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<TSource[]> list, Func<TSource, bool> func) => list.ForLinq().TakeWhile(func);

        public static Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<TSource[]> list, Func<TSource, int, bool> func) => list.ForLinq().TakeWhile(func);

        public static Task<TSource> Aggregate<TSource>(
        this Task<TSource[]> list, Func<TSource, TSource, TSource> func)
            => list.ForLinq().Aggregate(func);

        public static Task<TAccumulate> Aggregate<TSource, TAccumulate>(
        this Task<TSource[]> list, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => list.ForLinq().Aggregate(seed, func);

        public static Task<TResult> Aggregate<TSource, TAccumulate, TResult>(
        this Task<TSource[]> list, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func1, Func<TAccumulate, TResult> func2) => list.ForLinq().Aggregate(seed, func1, func2);

        public static Task<bool> All<TSource>(this Task<TSource[]> list, Func<TSource, bool> func)
            => list.ForLinq().All(func);

        public static Task<bool> Any<TSource>(this Task<TSource[]> list)
            => list.ForLinq().Any();

        public static Task<bool> Any<TSource>(this Task<TSource[]> list, Func<TSource, bool> func)
            => list.ForLinq().Any(func);

        public static Task<bool> Any<TSource>(this Task<TSource[]> list, Func<TSource, Task<bool>> func)
            => list.ForLinq().Any(func);

        public static Task<bool> Any<TSource>(this Task<TSource[]> list, Func<TSource, int, bool> func)
            => list.ForLinq().Any(func);

        public static Task<bool> Contains<TSource>(this Task<TSource[]> list, TSource item)
            => list.ForLinq().Contains(item);

        public static Task<bool> Contains<TSource>(this Task<TSource[]> list, Task<TSource> item)
            => list.ForLinq().Contains(item);

        public static Task<decimal> Average<TSource>(this Task<TSource[]> list, Func<TSource, decimal> func)
            => list.ForLinq().Average(func);

        public static Task<int> Count<TSource>(
        this Task<TSource[]> list, Func<TSource, bool> func) => list.ForLinq().Count(func);

        public static Task<int> Count<TSource>(
        this Task<TSource[]> list, Func<TSource, int, bool> func) => list.ForLinq().Count(func);

        public static Task<decimal> Sum<TSource>(
        this Task<TSource[]> list, Func<TSource, decimal> func) => list.ForLinq().Sum(func);

        public static Task<TimeSpan> Sum<TSource>(
        this Task<TSource[]> list, Func<TSource, TimeSpan> func) => list.ForLinq().Sum(func);

        public static Task<TResult> Max<TSource, TResult>(
        this Task<TSource[]> list, Func<TSource, TResult> func) => list.ForLinq().Max(func);

        public static Task<TResult> Min<TSource, TResult>(
        this Task<TSource[]> list, Func<TSource, TResult> func) => list.ForLinq().Min(func);

        public static async Task<IEnumerable<TResult>> Cast<TResult>(this Task<Array> list)
            => (await list)?.Cast<TResult>();

        public static async Task<IEnumerable<TResult>> OfType<TResult>(this Task<Array> list)
            => (await list)?.OfType<TResult>();
    }
}