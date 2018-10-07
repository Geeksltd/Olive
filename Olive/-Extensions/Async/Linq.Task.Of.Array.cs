﻿/* 
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
          this Task<TSource[]> @this, Func<TSource, TResult> func)
            => @this.ForLinq().Select(func);

        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, Task<TResult>> func)
            => @this.ForLinq().Select(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, IEnumerable<TResult>> func)
            => @this.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, IEnumerable<Task<TResult>>> func)
            => @this.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, Task<IEnumerable<TResult>>> func)
            => @this.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TSource>> Except<TSource>(
          this Task<TSource[]> @this, Func<TSource, bool> func)
            => @this.ForLinq().Except(func);

        public static Task<IEnumerable<TSource>> ExceptNull<TSource>(this Task<TSource[]> @this)
           => @this.ForLinq().ExceptNull();

        public static Task<IEnumerable<TResult>> Cast<TSource, TResult>(this Task<TSource[]> @this)
            => @this.ForLinq().Cast<TSource, TResult>();

        public static Task<IEnumerable<TSource>> Concat<TSource>(
          this Task<TSource[]> @this, IEnumerable<TSource> second)
            => @this.ForLinq().Concat(second);

        public static Task<IEnumerable<TSource>> Distinct<TSource, TResult>(this Task<TSource[]> @this, Func<TSource, TResult> func)
            => @this.ForLinq().Distinct(func);

        public static Task<IEnumerable<TSource>> Distinct<TSource>(this Task<TSource[]> @this)
            => @this.ForLinq().Distinct();

        public static Task<TSource> First<TSource>(
          this Task<TSource[]> @this, Func<TSource, bool> func)
            => @this.ForLinq().First(func);

        public static Task<TSource> FirstOrDefault<TSource>(
          this Task<TSource[]> @this, Func<TSource, bool> func)
            => @this.ForLinq().FirstOrDefault(func);

        public static Task<IEnumerable<TSource>> Intersect<TSource>(
        this Task<TSource[]> @this, IEnumerable<TSource> second) => @this.ForLinq().Intersect(second);

        public static Task<TSource> Last<TSource>(
        this Task<TSource[]> @this, Func<TSource, bool> func) => @this.ForLinq().Last(func);

        public static Task<TSource> LastOrDefault<TSource>(
        this Task<TSource[]> @this, Func<TSource, bool> func) => @this.ForLinq().LastOrDefault(func);

        public static Task<IOrderedEnumerable<TSource>> OrderBy<TSource, TKey>(
        this Task<TSource[]> @this, Func<TSource, TKey> func) => @this.ForLinq().OrderBy(func);

        public static Task<IOrderedEnumerable<TSource>> OrderByDescending<TSource, TKey>(
        this Task<TSource[]> @this, Func<TSource, TKey> func) => @this.ForLinq().OrderByDescending(func);

        public static Task<IEnumerable<TSource>> Reverse<TSource>(
        this Task<TSource[]> @this) => @this.ForLinq().Reverse();

        public static Task<TSource> Single<TSource>(
        this Task<TSource[]> @this, Func<TSource, bool> func) => @this.ForLinq().Single(func);

        public static Task<TSource> SingleOrDefault<TSource>(
        this Task<TSource[]> @this, Func<TSource, bool> func) => @this.ForLinq().SingleOrDefault(func);

        public static Task<IEnumerable<TSource>> Union<TSource>(
        this Task<TSource[]> @this, IEnumerable<TSource> second) => @this.ForLinq().Union(second);

        public static Task<IEnumerable<TSource>> Where<TSource>(
        this Task<TSource[]> @this, Func<TSource, bool> func) => @this.ForLinq().Where(func);

        public static Task<IEnumerable<TSource>> Where<TSource>(
        this Task<TSource[]> @this, Func<TSource, int, bool> func) => @this.ForLinq().Where(func);

        public static Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(
        this Task<TSource[]> @this, IEnumerable<TSecond> second, Func<TSource, TSecond, TResult> func)
            => @this.ForLinq().Zip(second, func);

        public static Task<IEnumerable<TSource>> Skip<TSource>(
        this Task<TSource[]> @this, int count) => @this.ForLinq().Skip(count);

        public static Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<TSource[]> @this, Func<TSource, bool> func) => @this.ForLinq().SkipWhile(func);

        public static Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<TSource[]> @this, Func<TSource, int, bool> func) => @this.ForLinq().SkipWhile(func);

        public static Task<IEnumerable<TSource>> Take<TSource>(
        this Task<TSource[]> @this, int count) => @this.ForLinq().Take(count);

        public static Task<IEnumerable<TSource>> Take<TSource>(
        this Task<TSource[]> @this, int lower, int count) => @this.ForLinq().Take(lower, count);

        public static Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<TSource[]> @this, Func<TSource, bool> func) => @this.ForLinq().TakeWhile(func);

        public static Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<TSource[]> @this, Func<TSource, int, bool> func) => @this.ForLinq().TakeWhile(func);

        public static Task<TSource> Aggregate<TSource>(
        this Task<TSource[]> @this, Func<TSource, TSource, TSource> func)
            => @this.ForLinq().Aggregate(func);

        public static Task<TAccumulate> Aggregate<TSource, TAccumulate>(
        this Task<TSource[]> @this, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => @this.ForLinq().Aggregate(seed, func);

        public static Task<TResult> Aggregate<TSource, TAccumulate, TResult>(
        this Task<TSource[]> @this, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func1, Func<TAccumulate, TResult> func2) => @this.ForLinq().Aggregate(seed, func1, func2);

        public static Task<bool> All<TSource>(this Task<TSource[]> @this, Func<TSource, bool> func)
            => @this.ForLinq().All(func);

        public static Task<bool> Any<TSource>(this Task<TSource[]> @this)
            => @this.ForLinq().Any();

        public static Task<bool> Any<TSource>(this Task<TSource[]> @this, Func<TSource, bool> func)
            => @this.ForLinq().Any(func);

        public static Task<bool> Any<TSource>(this Task<TSource[]> @this, Func<TSource, Task<bool>> func)
            => @this.ForLinq().Any(func);

        public static Task<bool> Any<TSource>(this Task<TSource[]> @this, Func<TSource, int, bool> func)
            => @this.ForLinq().Any(func);

        public static Task<bool> Contains<TSource>(this Task<TSource[]> @this, TSource item)
            => @this.ForLinq().Contains(item);

        public static Task<bool> Contains<TSource>(this Task<TSource[]> @this, Task<TSource> item)
            => @this.ForLinq().Contains(item);

        public static Task<decimal> Average<TSource>(this Task<TSource[]> @this, Func<TSource, decimal> func)
            => @this.ForLinq().Average(func);

        public static Task<int> Count<TSource>(
        this Task<TSource[]> @this, Func<TSource, bool> func) => @this.ForLinq().Count(func);

        public static Task<int> Count<TSource>(
        this Task<TSource[]> @this, Func<TSource, int, bool> func) => @this.ForLinq().Count(func);

        public static Task<decimal> Sum<TSource>(
        this Task<TSource[]> @this, Func<TSource, decimal> func) => @this.ForLinq().Sum(func);

        public static Task<TimeSpan> Sum<TSource>(
        this Task<TSource[]> @this, Func<TSource, TimeSpan> func) => @this.ForLinq().Sum(func);

        public static Task<TResult> Max<TSource, TResult>(
        this Task<TSource[]> @this, Func<TSource, TResult> func) => @this.ForLinq().Max(func);

        public static Task<TResult> Min<TSource, TResult>(
        this Task<TSource[]> @this, Func<TSource, TResult> func) => @this.ForLinq().Min(func);

        public static Task<IEnumerable<TResult>> Cast<TResult>(this Task<Array> @this)
          => @this.Get(x => x.Cast<TResult>());

        public static Task<IEnumerable<TResult>> OfType<TResult>(this Task<Array> @this)
            => @this.Get(x => x.OfType<TResult>());

        /// <summary>
        /// If a specified condition is true, then the filter predicate will be executed.
        /// Otherwise the original list will be returned.
        /// </summary>
        [EscapeGCop("The condition param should not be last in this case.")]
        public static Task<IEnumerable<T>> FilterIf<T>(this Task<T[]> source,
             bool condition, Func<T, bool> predicate)
            => condition ? source.ForLinq().Where(predicate) : source.ForLinq();
    }
}