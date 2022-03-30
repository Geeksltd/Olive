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
        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, TResult> func)
            => @this.Get(x => x.OrEmpty().Select(func));

        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, Task<TResult>> func)
            => @this.Get(x => x.OrEmpty().SequentialSelect(func));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, IEnumerable<TResult>> func)
            => @this.Get(x => x.OrEmpty().SelectMany(func));

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, IEnumerable<Task<TResult>>> func)
        {
            var source = await @this.Get(x => x.OrEmpty().SelectMany(func)).ConfigureAwait(false);
            return await source.SequentialSelect(x => x).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, Task<IEnumerable<TResult>>> func)
        {
            var source = await @this.Get(x => x.OrEmpty().Select(func)).ConfigureAwait(false);
            return await source.SequentialSelect(x => x).SelectMany(v => v).ConfigureAwait(false);
        }

        public static Task<IEnumerable<TSource>> Except<TSource>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().Except(func));

        public static Task<IEnumerable<TSource>> Except<TSource>(
         this Task<IEnumerable<TSource>> @this, Func<TSource, Task<bool>> func)
           => @this.Get(x => x.OrEmpty().Except(func));

        public static Task<IEnumerable<TSource>> Except<TSource>(
        this Task<IEnumerable<TSource>> @this, IEnumerable<TSource> exclusion)
          => @this.Get(x => x.OrEmpty().Except(exclusion));

        public static Task<IEnumerable<TSource>> ExceptNull<TSource>(
         this Task<IEnumerable<TSource>> @this) => @this.Where(x => x is not null);

        public static Task<IEnumerable<TResult>> OfType<TSource, TResult>(this Task<IEnumerable<TSource>> @this)
            => @this.Get(x => x.OrEmpty().OfType<TResult>());

        public static Task<IEnumerable<TResult>> OfType<TResult>(this Task<IEnumerable> @this)
            => @this.Get(x => x.OrEmpty().OfType<TResult>());

        public static Task<IEnumerable<TResult>> Cast<TSource, TResult>(this Task<IEnumerable<TSource>> @this)
            => @this.Get(x => x.OrEmpty().Cast<TResult>());

        public static Task<IEnumerable<TSource>> Concat<TSource>(
          this Task<IEnumerable<TSource>> @this, IEnumerable<TSource> second)
            => @this.Get(x => x.OrEmpty().Concat(second));

        public static Task<IEnumerable<TSource>> Concat<TSource>(
          this Task<IEnumerable<TSource>> @this, TSource item)
            => @this.Get(x => x.OrEmpty().Concat(item));

        public static Task<IEnumerable<TSource>> Distinct<TSource>(
          this Task<IEnumerable<TSource>> @this) => @this.Get(x => x.OrEmpty().Distinct());

        public static Task<IEnumerable<TSource>> Distinct<TSource, TResult>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, TResult> func)
            => @this.Get(x => x.OrEmpty().Distinct(func));

        public static Task<IEnumerable<IGrouping<TKey, TSource>>> GroupBy<TSource, TKey>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, TKey> groupBy)
          => @this.Get(x => x.GroupBy(groupBy));

        public static Task<TSource> First<TSource>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().First(func));

        public static Task<TSource> FirstOrDefault<TSource>(
          this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().FirstOrDefault(func));

        public static Task<TSource> First<TSource>(this Task<IEnumerable<TSource>> @this)
            => @this.Get(x => x.OrEmpty().First());

        public static Task<TSource> FirstOrDefault<TSource>(this Task<IEnumerable<TSource>> @this)
            => @this.Get(x => x.OrEmpty().FirstOrDefault());

        public static Task<TSource> Last<TSource>(this Task<IEnumerable<TSource>> @this)
            => @this.Get(x => x.OrEmpty().Last());

        public static Task<TSource> LastOrDefault<TSource>(this Task<IEnumerable<TSource>> @this)
            => @this.Get(x => x.OrEmpty().LastOrDefault());

        public static Task<IEnumerable<TSource>> Intersect<TSource>(
        this Task<IEnumerable<TSource>> @this, IEnumerable<TSource> second)
            => @this.Get(x => x.OrEmpty().Intersect(second));

        public static Task<TSource> Last<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().Last(func));

        public static Task<TSource> LastOrDefault<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().LastOrDefault(func));

        public static Task<IOrderedEnumerable<TSource>> OrderBy<TSource, TKey>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, TKey> func)
            => @this.Get(x => x.OrEmpty().OrderBy(func));

        public static Task<IOrderedEnumerable<TSource>> OrderByDescending<TSource, TKey>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, TKey> func)
            => @this.Get(x => x.OrEmpty().OrderByDescending(func));

        public static Task<IEnumerable<TSource>> Reverse<TSource>(
        this Task<IEnumerable<TSource>> @this) => @this.Get(x => x.OrEmpty().Reverse());

        public static Task<TSource> Single<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().Single(func));

        public static Task<TSource> Single<TSource>(this Task<IEnumerable<TSource>> @this)
           => @this.Get(x => x.OrEmpty().Single());

        public static Task<TSource> SingleOrDefault<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().SingleOrDefault(func));

        public static Task<TSource> SingleOrDefaultAsync<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, Task<bool>> func)
         => @this.Get(x => x.OrEmpty().SingleOrDefault(func));

        public static Task<IEnumerable<TSource>> Union<TSource>(
        this Task<IEnumerable<TSource>> @this, IEnumerable<TSource> second)
            => @this.Get(x => x.OrEmpty().Union(second));

        public static Task<IEnumerable<TSource>> Where<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().Where(func));

        public static Task<IEnumerable<TSource>> Where<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, int, bool> func)
            => @this.Get(x => x.OrEmpty().Where(func));

        public static Task<IEnumerable<TSource>> WhereAsync<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, Task<bool>> func)
         => @this.Get(x => x.OrEmpty().Where(func));

        public static Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(
        this Task<IEnumerable<TSource>> @this, IEnumerable<TSecond> second, Func<TSource, TSecond, TResult> func)
            => @this.Get(x => x.OrEmpty().Zip(second, func));

        public static Task<IEnumerable<TSource>> Skip<TSource>(
        this Task<IEnumerable<TSource>> @this, int count) => @this.Get(x => x.OrEmpty().Skip(count));

        public static Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().SkipWhile(func));

        public static Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, int, bool> func)
            => @this.Get(x => x.OrEmpty().SkipWhile(func));

        public static Task<IEnumerable<TSource>> Take<TSource>(
        this Task<IEnumerable<TSource>> @this, int count)
            => @this.Get(x => x.OrEmpty().Take(count));

        public static Task<IEnumerable<TSource>> Take<TSource>(
        this Task<IEnumerable<TSource>> @this, int lower, int count)
            => @this.Get(x => x.OrEmpty().Take(lower, count));

        public static Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().TakeWhile(func));

        public static Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, int, bool> func)
            => @this.Get(x => x.OrEmpty().TakeWhile(func));

        public static Task<TSource> Aggregate<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, TSource, TSource> func)
            => @this.Get(x => x.OrEmpty().Aggregate(func));

        public static Task<TAccumulate> Aggregate<TSource, TAccumulate>(
        this Task<IEnumerable<TSource>> @this, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => @this.Get(x => x.OrEmpty().Aggregate(seed, func));

        public static Task<TResult> Aggregate<TSource, TAccumulate, TResult>(
        this Task<IEnumerable<TSource>> @this, TAccumulate seed,
        Func<TAccumulate, TSource, TAccumulate> func1,
        Func<TAccumulate, TResult> func2)
            => @this.Get(x => x.OrEmpty().Aggregate(seed, func1, func2));

        public static Task<bool> All<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().All(func));

        public static Task<bool> Any<TSource>(this Task<IEnumerable<TSource>> @this)
            => @this.Get(x => x.OrEmpty().Any());

        public static Task<bool> Any<TSource>(this Task<IEnumerable<TSource>> @this,
            Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().Any(func));

        public static Task<bool> Any<TSource>(this Task<IEnumerable<TSource>> @this,
            Func<TSource, Task<bool>> func)
            => @this.Get(x => x.OrEmpty().Any(func));

        public static Task<bool> Any<TSource>(this Task<IEnumerable<TSource>> @this,
            Func<TSource, int, bool> func)
            => @this.Get(x => x.OrEmpty().Any(func));

        public static Task<bool> None<TSource>(this Task<IEnumerable<TSource>> @this)
          => @this.Get(x => x.OrEmpty().None());

        public static Task<bool> None<TSource>(this Task<IEnumerable<TSource>> @this,
            Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().None(func));

        public static Task<bool> None<TSource>(this Task<IEnumerable<TSource>> @this,
            Func<TSource, Task<bool>> func)
            => @this.Get(x => x.OrEmpty().None(func));

        public static Task<bool> None<TSource>(this Task<IEnumerable<TSource>> @this,
            Func<TSource, int, bool> func)
            => @this.Get(x => x.OrEmpty().None(func));

        public static Task<bool> Contains<TSource>(this Task<IEnumerable<TSource>> @this, TSource item)
            => @this.Get(x => x.OrEmpty().Contains(item));

        public static Task<bool> Contains<TSource>(this Task<IEnumerable<TSource>> @this,
            Task<TSource> item)
            => @this.Get(x => x.OrEmpty().Contains(item));

        public static Task<decimal> Average<TSource>(this Task<IEnumerable<TSource>> @this,
            Func<TSource, decimal> func)
            => @this.Get(x => x.OrEmpty().Average(func));

        public static Task<int> Count<TSource>(this Task<IEnumerable<TSource>> @this)
            => @this.Get(x => x.OrEmpty().Count());

        public static Task<int> Count<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.Get(x => x.OrEmpty().Count(func));

        public static Task<int> Count<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, int, bool> func)
            => @this.Get(x => x.OrEmpty().Count(func));

        public static Task<decimal> Sum<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, decimal> func)
            => @this.Get(x => x.OrEmpty().Sum(func));

        public static Task<TimeSpan> Sum<TSource>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, TimeSpan> func)
            => @this.Get(x => x.OrEmpty().Sum(func));

        public static Task<TResult> Max<TSource, TResult>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, TResult> func)
            => @this.Get(x => x.OrEmpty().Max(func));

        public static Task<TResult> Min<TSource, TResult>(
        this Task<IEnumerable<TSource>> @this, Func<TSource, TResult> func)
            => @this.Get(v => v.OrEmpty().Min(func));

        /// <summary>
        /// Selects the item with minimum of the specified value.
        /// </summary>
        public static Task<T> WithMin<T, TKey>(this Task<IEnumerable<T>> @this, Func<T, TKey> keySelector)
            => @this.Get(v => v.OrEmpty().WithMin(keySelector));

        /// <summary>
        /// Selects the item with maximum of the specified value.
        /// </summary>
        public static Task<T> WithMax<T, TKey>(this Task<IEnumerable<T>> @this, Func<T, TKey> keySelector)
            => @this.Get(v => v.OrEmpty().WithMax(keySelector));

        public static Task<IEnumerable<TResult>> Cast<TResult>(this Task<IEnumerable> @this)
            => @this.Get(x => x.Cast<TResult>());

        public static Task<bool> HasMany<TSource>(this Task<IEnumerable<TSource>> @this)
            => @this.Get(x => x.HasMany());

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<IEnumerable<TSource>> @this, TSource item)
            => @this.Get(x => x.Except(item));

        public static Task<IOrderedEnumerable<T>> ThenBy<T, TKey>(this Task<IOrderedEnumerable<T>> @this, Func<T, TKey> keySelector)
            => @this.Get(x => x.ThenBy(keySelector));

        public static Task<IOrderedEnumerable<T>> ThenByDescending<T, TKey>(this Task<IOrderedEnumerable<T>> @this, Func<T, TKey> keySelector)
            => @this.Get(x => x.ThenByDescending(keySelector));

        public static Task<IEnumerable<TSource>> Concat<TSource>(this Task<IEnumerable<TSource>> @this,
           Task<IEnumerable<TSource>> other)
            => @this.Get(x => x.Concat(other));

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<IEnumerable<TSource>> @this,
           Task<IEnumerable<TSource>> exclude)
            => exclude.Get(x => @this.Except(x));

        /// <summary>
        /// If a specified condition is true, then the filter predicate will be executed.
        /// Otherwise the original list will be returned.
        /// </summary>
        [EscapeGCop("The condition param should not be last in this case.")]
        public static Task<IEnumerable<T>> FilterIf<T>(this Task<IEnumerable<T>> source,
             bool condition, Func<T, bool> predicate)
            => condition ? source.Where(predicate) : source;

        /// <summary>
        /// Returns an empty List if this collection is null.
        /// </summary>
        [EscapeGCop("I am the GCop solution")]
        public static async Task<IEnumerable<T>> OrEmpty<T>(this Task<IEnumerable<T>> @this)
        {
            return (await @this.ConfigureAwait(false)) ?? Enumerable.Empty<T>();
        }
    }
}