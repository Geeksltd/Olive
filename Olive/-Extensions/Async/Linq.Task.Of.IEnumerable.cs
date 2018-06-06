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
          this Task<IEnumerable<TSource>> @this, Func<TSource, TResult> func)
            => (await @this).OrEmpty().Select(func);

        public static async Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, Task<TResult>> func)
            => await (await @this).OrEmpty().Select(func).AwaitAll();

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, IEnumerable<TResult>> func)
            => (await @this).OrEmpty().SelectMany(func);

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, IEnumerable<Task<TResult>>> func)
            => await (await @this).OrEmpty().SelectMany(func).AwaitAll();

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, Task<IEnumerable<TResult>>> func)
            => await (await @this).OrEmpty().Select(func).AwaitAll().SelectMany(x => x);

        public static async Task<IEnumerable<TSource>> Except<TSource>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().Except(func);

        public static Task<IEnumerable<TSource>> ExceptNull<TSource>(
         this Task<IEnumerable<TSource>> @this) => @this.Where(x => !ReferenceEquals(x, null));

        public static async Task<IEnumerable<TResult>> Cast<TSource, TResult>(this Task<IEnumerable<TSource>> @this)
            => (await @this).OrEmpty().Cast<TResult>();

        public static async Task<IEnumerable<TSource>> Concat<TSource>(
          this Task<IEnumerable<TSource>> @this, IEnumerable<TSource> second)
            => (await @this).OrEmpty().Concat(second);

        public static async Task<IEnumerable<TSource>> Concat<TSource>(
          this Task<IEnumerable<TSource>> @this, TSource item)
            => (await @this).OrEmpty().Concat(item);

        public static async Task<IEnumerable<TSource>> Distinct<TSource>(
          this Task<IEnumerable<TSource>> @this) => (await @this).OrEmpty().Distinct();

        public static async Task<IEnumerable<TSource>> Distinct<TSource, TResult>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, TResult> func)
            => (await @this).OrEmpty().Distinct(func);

        public static async Task<TSource> First<TSource>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().First(func);

        public static async Task<TSource> FirstOrDefault<TSource>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().FirstOrDefault(func);

        public static Task<TSource> First<TSource>(this Task<IEnumerable<TSource>> @this)
        {
            return @this.First(x => true);
        }

        public static Task<TSource> FirstOrDefault<TSource>(this Task<IEnumerable<TSource>> @this)
        {
            return @this.FirstOrDefault(x => true);
        }

        public static async Task<IEnumerable<TSource>> Intersect<TSource>(
        this Task<IEnumerable<TSource>> @this, IEnumerable<TSource> second)
            => (await @this).OrEmpty().Intersect(second);

        public static async Task<TSource> Last<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().Last(func);

        public static async Task<TSource> LastOrDefault<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().LastOrDefault(func);

        public static async Task<IOrderedEnumerable<TSource>> OrderBy<TSource, TKey>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, TKey> func)
            => (await @this).OrEmpty().OrderBy(func);

        public static async Task<IOrderedEnumerable<TSource>> OrderByDescending<TSource, TKey>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, TKey> func)
            => (await @this).OrEmpty().OrderByDescending(func);

        public static async Task<IEnumerable<TSource>> Reverse<TSource>(
        this Task<IEnumerable<TSource>> @this) => (await @this).OrEmpty().Reverse();

        public static async Task<TSource> Single<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().Single(func);

        public static async Task<TSource> SingleOrDefault<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().SingleOrDefault(func);

        public static async Task<IEnumerable<TSource>> Union<TSource>(
        this Task<IEnumerable<TSource>> @this, IEnumerable<TSource> second)
            => (await @this).OrEmpty().Union(second);

        public static async Task<IEnumerable<TSource>> Where<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().Where(func);

        public static async Task<IEnumerable<TSource>> Where<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, int, bool> func)
            => (await @this).OrEmpty().Where(func);

        public static async Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(
        this Task<IEnumerable<TSource>> @this, IEnumerable<TSecond> second, Func<TSource, TSecond, TResult> func)
            => (await @this).OrEmpty().Zip(second, func);

        public static async Task<IEnumerable<TSource>> Skip<TSource>(
        this Task<IEnumerable<TSource>> @this, int count) => (await @this).OrEmpty().Skip(count);

        public static async Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().SkipWhile(func);

        public static async Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, int, bool> func)
            => (await @this).OrEmpty().SkipWhile(func);

        public static async Task<IEnumerable<TSource>> Take<TSource>(
        this Task<IEnumerable<TSource>> @this, int count)
            => (await @this).OrEmpty().Take(count);

        public static async Task<IEnumerable<TSource>> Take<TSource>(
        this Task<IEnumerable<TSource>> @this, int lower, int count)
            => (await @this).OrEmpty().Take(lower, count);

        public static async Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().TakeWhile(func);

        public static async Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, int, bool> func)
            => (await @this).OrEmpty().TakeWhile(func);

        public static async Task<TSource> Aggregate<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, TSource, TSource> func)
            => (await @this).OrEmpty().Aggregate(func);

        public static async Task<TAccumulate> Aggregate<TSource, TAccumulate>(
        this Task<IEnumerable<TSource>> @this, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => (await @this).OrEmpty().Aggregate(seed, func);

        public static async Task<TResult> Aggregate<TSource, TAccumulate, TResult>(
        this Task<IEnumerable<TSource>> @this, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func1, Func<TAccumulate, TResult> func2) => (await @this).OrEmpty().Aggregate(seed, func1, func2);

        public static async Task<bool> All<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().All(func);

        public static async Task<bool> Any<TSource>(this Task<IEnumerable<TSource>> list)
            => (await list).OrEmpty().Any();

        public static async Task<bool> Any<TSource>(this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().Any(func);

        public static async Task<bool> Any<TSource>(this Task<IEnumerable<TSource>> @this, Func<TSource, Task<bool>> func)
            => await (await @this).OrEmpty().Any(func);

        public static async Task<bool> Any<TSource>(this Task<IEnumerable<TSource>> @this, Func<TSource, int, bool> func)
            => (await @this).OrEmpty().Any(func);

        public static async Task<bool> None<TSource>(this Task<IEnumerable<TSource>> @this)
          => (await @this).OrEmpty().None();

        public static async Task<bool> None<TSource>(this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().None(func);

        public static async Task<bool> None<TSource>(this Task<IEnumerable<TSource>> @this, Func<TSource, Task<bool>> func)
            => await (await @this).OrEmpty().None(func);

        public static async Task<bool> None<TSource>(this Task<IEnumerable<TSource>> @this, Func<TSource, int, bool> func)
            => (await @this).OrEmpty().None(func);

        public static async Task<bool> Contains<TSource>(this Task<IEnumerable<TSource>> @this, TSource item)
            => (await @this).OrEmpty().Contains(item);

        public static async Task<bool> Contains<TSource>(this Task<IEnumerable<TSource>> @this, Task<TSource> item)
            => (await @this).OrEmpty().Contains(await item);

        public static async Task<decimal> Average<TSource>(this Task<IEnumerable<TSource>> @this, Func<TSource, decimal> func)
            => (await @this).OrEmpty().Average(func);

        public static async Task<int> Count<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => (await @this).OrEmpty().Count(func);

        public static async Task<int> Count<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, int, bool> func)
            => (await @this).OrEmpty().Count(func);

        public static async Task<decimal> Sum<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, decimal> func)
            => (await @this).OrEmpty().Sum(func);

        public static async Task<TimeSpan> Sum<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, TimeSpan> func)
            => (await @this).OrEmpty().Sum(func);

        public static async Task<TResult> Max<TSource, TResult>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, TResult> func)
            => (await @this).OrEmpty().Max(func);

        public static async Task<TResult> Min<TSource, TResult>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, TResult> func)
            => (await @this).OrEmpty().Min(func);

        public static async Task<IEnumerable<TResult>> Cast<TResult>(this Task<IEnumerable> @this)
            => (await @this).Cast<TResult>();

        public static async Task<IEnumerable<TResult>> OfType<TResult>(this Task<IEnumerable> @this)
            => (await @this).OfType<TResult>();
    }
}