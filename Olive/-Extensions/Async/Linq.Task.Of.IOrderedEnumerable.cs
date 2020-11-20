/* 
 * Linq methods that are:
 *       defined on Task<IOrderedEnumerable<T>>
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
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TResult> func)
            => @this.ForLinq().Select(func);

        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, Task<TResult>> func)
            => @this.ForLinq().Select(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, IEnumerable<TResult>> func)
            => @this.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, IEnumerable<Task<TResult>>> func)
            => @this.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, Task<IEnumerable<TResult>>> func)
            => @this.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TSource>> Except<TSource>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().Except(func);

        public static Task<IEnumerable<TSource>> ExceptNull<TSource>(this Task<IOrderedEnumerable<TSource>> @this)
           => @this.ForLinq().ExceptNull();

        public static Task<IEnumerable<TResult>> Cast<TSource, TResult>(this Task<IOrderedEnumerable<TSource>> @this)
            => @this.ForLinq().Cast<TSource, TResult>();

        public static Task<IEnumerable<TSource>> Concat<TSource>(
          this Task<IOrderedEnumerable<TSource>> @this, IEnumerable<TSource> second)
            => @this.ForLinq().Concat(second);

        public static Task<IEnumerable<TSource>> Distinct<TSource, TResult>(this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TResult> func)
            => @this.ForLinq().Distinct(func);

        public static Task<IEnumerable<TSource>> Distinct<TSource>(this Task<IOrderedEnumerable<TSource>> @this)
            => @this.ForLinq().Distinct();

        public static Task<TSource> First<TSource>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().First(func);

        public static Task<TSource> FirstOrDefault<TSource>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().FirstOrDefault(func);

        public static Task<IEnumerable<TSource>> Intersect<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, IEnumerable<TSource> second) => @this.ForLinq().Intersect(second);

        public static Task<TSource> Last<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().Last(func);

        public static Task<TSource> LastOrDefault<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().LastOrDefault(func);

        public static Task<IOrderedEnumerable<TSource>> ThenBy<TSource, TKey>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TKey> func) => @this.ForLinq().OrderBy(func);

        public static Task<IOrderedEnumerable<TSource>> ThenByDescending<TSource, TKey>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TKey> func) => @this.ForLinq().OrderByDescending(func);

        public static Task<IEnumerable<TSource>> Reverse<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this) => @this.ForLinq().Reverse();

        public static Task<TSource> Single<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().Single(func);

        public static Task<TSource> SingleOrDefault<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().SingleOrDefault(func);

        public static Task<IEnumerable<TSource>> Union<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, IEnumerable<TSource> second) => @this.ForLinq().Union(second);

        public static Task<IEnumerable<TSource>> Where<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().Where(func);

        public static Task<IEnumerable<TSource>> Where<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, int, bool> func) => @this.ForLinq().Where(func);

        public static Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(
        this Task<IOrderedEnumerable<TSource>> @this, IEnumerable<TSecond> second, Func<TSource, TSecond, TResult> func)
            => @this.ForLinq().Zip(second, func);

        public static Task<IEnumerable<TSource>> Skip<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, int count) => @this.ForLinq().Skip(count);

        public static Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().SkipWhile(func);

        public static Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, int, bool> func) => @this.ForLinq().SkipWhile(func);

        public static Task<IEnumerable<TSource>> Take<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, int count) => @this.ForLinq().Take(count);

        public static Task<IEnumerable<TSource>> Take<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, int lower, int count) => @this.ForLinq().Take(lower, count);

        public static Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().TakeWhile(func);

        public static Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, int, bool> func) => @this.ForLinq().TakeWhile(func);

        public static Task<TSource> Aggregate<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TSource, TSource> func)
            => @this.ForLinq().Aggregate(func);

        public static Task<TAccumulate> Aggregate<TSource, TAccumulate>(
        this Task<IOrderedEnumerable<TSource>> @this, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => @this.ForLinq().Aggregate(seed, func);

        public static Task<TResult> Aggregate<TSource, TAccumulate, TResult>(
        this Task<IOrderedEnumerable<TSource>> @this, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func1, Func<TAccumulate, TResult> func2) => @this.ForLinq().Aggregate(seed, func1, func2);

        public static Task<bool> All<TSource>(this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().All(func);

        public static Task<bool> Any<TSource>(this Task<IOrderedEnumerable<TSource>> @this)
            => @this.ForLinq().Any();

        public static Task<bool> Any<TSource>(this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().Any(func);

        public static Task<bool> Any<TSource>(this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, Task<bool>> func)
            => @this.ForLinq().Any(func);

        public static Task<bool> Any<TSource>(this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, int, bool> func)
            => @this.ForLinq().Any(func);

        public static Task<bool> Contains<TSource>(this Task<IOrderedEnumerable<TSource>> @this, TSource item)
            => @this.ForLinq().Contains(item);

        public static Task<bool> Contains<TSource>(this Task<IOrderedEnumerable<TSource>> @this, Task<TSource> item)
            => @this.ForLinq().Contains(item);

        public static Task<decimal> Average<TSource>(this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, decimal> func)
            => @this.ForLinq().Average(func);

        public static Task<int> Count<TSource>(this Task<IOrderedEnumerable<TSource>> @this) => @this.ForLinq().Count();

        public static Task<int> Count<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().Count(func);

        public static Task<int> Count<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, int, bool> func) => @this.ForLinq().Count(func);

        public static Task<decimal> Sum<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, decimal> func) => @this.ForLinq().Sum(func);

        public static Task<TimeSpan> Sum<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TimeSpan> func) => @this.ForLinq().Sum(func);

        public static Task<TResult> Max<TSource, TResult>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TResult> func) => @this.ForLinq().Max(func);

        public static Task<TResult> Min<TSource, TResult>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TResult> func) => @this.ForLinq().Min(func);

        public static Task<List<TSource>> ToList<TSource>(this Task<IOrderedEnumerable<TSource>> @this)
            => @this.ForLinq().ToList();

        public static Task<TSource[]> ToArray<TSource>(this Task<IOrderedEnumerable<TSource>> @this)
            => @this.ForLinq().ToArray();
    }
}