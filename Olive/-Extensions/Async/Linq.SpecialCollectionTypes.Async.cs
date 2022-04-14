using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Olive
{
    partial class OliveExtensions
    {
              public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, TResult[]> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, Task<TResult[]>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, IOrderedEnumerable<TResult>> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, Task<IOrderedEnumerable<TResult>>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, List<TResult>> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, Task<List<TResult>>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, IList<TResult>> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, Task<IList<TResult>>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TResult[]> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, Task<TResult[]>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, IOrderedEnumerable<TResult>> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, Task<IOrderedEnumerable<TResult>>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, List<TResult>> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, Task<List<TResult>>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, IList<TResult>> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, Task<IList<TResult>>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<List<TSource>> @this, Func<TSource, TResult[]> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<List<TSource>> @this, Func<TSource, Task<TResult[]>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<List<TSource>> @this, Func<TSource, IOrderedEnumerable<TResult>> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<List<TSource>> @this, Func<TSource, Task<IOrderedEnumerable<TResult>>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<List<TSource>> @this, Func<TSource, List<TResult>> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<List<TSource>> @this, Func<TSource, Task<List<TResult>>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<List<TSource>> @this, Func<TSource, IList<TResult>> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<List<TSource>> @this, Func<TSource, Task<IList<TResult>>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IList<TSource>> @this, Func<TSource, TResult[]> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IList<TSource>> @this, Func<TSource, Task<TResult[]>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IList<TSource>> @this, Func<TSource, IOrderedEnumerable<TResult>> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IList<TSource>> @this, Func<TSource, Task<IOrderedEnumerable<TResult>>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IList<TSource>> @this, Func<TSource, List<TResult>> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IList<TSource>> @this, Func<TSource, Task<List<TResult>>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                  public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IList<TSource>> @this, Func<TSource, IList<TResult>> func)
            => @this.ForLinq().SelectMany(x=>func(x));

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IList<TSource>> @this, Func<TSource, Task<IList<TResult>>> func)
            => @this.ForLinq().SequentialSelectMany(x=>func(x).ForLinq());
                        


            #region TSource[]
        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, TResult> func)
            => @this.Get(x => x.OrEmpty().Select(func));

        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<TSource[]> @this, Func<TSource, Task<TResult>> func)
            => @this.ForLinq().Select(func);

        /// <summary>
        /// Selects the requested items one by one rather than in parallel. Use this in database operations to prevent over-concurrency.
        /// </summary>
        public static Task<IEnumerable<TResult>> SequentialSelect<TSource, TResult>(
            this Task<TSource[]> @this, Func<TSource, Task<TResult>> selector)
            => @this.ForLinq().SequentialSelect(selector);

        public static Task<IEnumerable<TSource>> WhereAsync<TSource>(
        this Task<TSource[]> @this, Func<TSource, Task<bool>> func)
         => @this.ForLinq().WhereAsync(func);

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

        public static Task<IEnumerable<TResult>> OfType<TSource, TResult>(this Task<TSource[]> @this)
            => @this.ForLinq().OfType<TSource, TResult>();

        public static Task<TSource> WithMin<TSource, TKey>(this Task<TSource[]> @this, Func<TSource, TKey> keySelector)
            => @this.Get(v => v.OrEmpty().WithMin(keySelector));

        public static Task<TSource> WithMax<TSource, TKey>(this Task<TSource[]> @this, Func<TSource, TKey> keySelector)
            => @this.Get(v => v.OrEmpty().WithMax(keySelector));

        public static Task<IEnumerable<TSource>> Concat<TSource>(
          this Task<TSource[]> @this, IEnumerable<TSource> second)
            => @this.ForLinq().Concat(second);

        public static Task<IEnumerable<TSource>> Distinct<TSource, TResult>(this Task<TSource[]> @this, Func<TSource, TResult> func)
            => @this.ForLinq().Distinct(func);

        public static Task<IEnumerable<TSource>> Distinct<TSource>(this Task<TSource[]> @this)
            => @this.ForLinq().Distinct();

        public static Task<IEnumerable<IGrouping<TKey, TSource>>> GroupBy<TSource, TKey>(
            this Task<TSource[]> @this, Func<TSource, TKey> groupBy)
            => @this.ForLinq().GroupBy(groupBy);

        public static Task<TSource> First<TSource>(
          this Task<TSource[]> @this, Func<TSource, bool> func)
            => @this.ForLinq().First(func);

        public static Task<TSource> First<TSource>(this Task<TSource[]> @this)
            => @this.ForLinq().First();

        public static Task<TSource> FirstOrDefault<TSource>(
          this Task<TSource[]> @this, Func<TSource, bool> func)
            => @this.ForLinq().FirstOrDefault(func);

        public static Task<TSource> FirstOrDefault<TSource>(this Task<TSource[]> @this)
            => @this.ForLinq().FirstOrDefault();

        public static Task<IEnumerable<TSource>> Intersect<TSource>(
        this Task<TSource[]> @this, IEnumerable<TSource> second) => @this.ForLinq().Intersect(second);

        public static Task<TSource> Last<TSource>(
        this Task<TSource[]> @this, Func<TSource, bool> func) => @this.ForLinq().Last(func);

        public static Task<TSource> LastOrDefault<TSource>(
        this Task<TSource[]> @this, Func<TSource, bool> func) => @this.ForLinq().LastOrDefault(func);

        public static Task<TSource> Last<TSource>(this Task<TSource[]> @this)
            => @this.ForLinq().Last();

        public static Task<TSource> LastOrDefault<TSource>(this Task<TSource[]> @this) 
            => @this.ForLinq().LastOrDefault();

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

        public static Task<TSource[]> ToArray<TSource>(this Task<TSource[]> @this) => @this.ForLinq().ToArray();
        
        public static Task<List<TSource>> ToList<TSource>(this Task<TSource[]> @this) => @this.ForLinq().ToList();

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

        public static Task<bool> None<TSource>(this Task<TSource[]> @this)
         => @this.ForLinq().None();

        public static Task<bool> None<TSource>(this Task<TSource[]> @this, Func<TSource, bool> func)
            => @this.ForLinq().None(func);

        public static Task<bool> None<TSource>(this Task<TSource[]> @this, Func<TSource, Task<bool>> func)
            => @this.ForLinq().None(func);

        public static Task<bool> None<TSource>(this Task<TSource[]> @this, Func<TSource, int, bool> func)
            => @this.ForLinq().None(func);

        public static Task<bool> Contains<TSource>(this Task<TSource[]> @this, TSource item)
            => @this.ForLinq().Contains(item);

        public static Task<bool> Contains<TSource>(this Task<TSource[]> @this, Task<TSource> item)
            => @this.ForLinq().Contains(item);

        public static Task<decimal> Average<TSource>(this Task<TSource[]> @this, Func<TSource, decimal> func)
            => @this.ForLinq().Average(func);

        public static Task<int> Count<TSource>(this Task<TSource[]> @this) => @this.ForLinq().Count();

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

        public static Task<IEnumerable<TSource>> OrEmpty<TSource>(this Task<TSource[]> @this) 
        => @this.ForLinq().OrEmpty() ?? Task.FromResult(Enumerable.Empty<TSource>());
 
        /// <summary>
        /// If a specified condition is true, then the filter predicate will be executed.
        /// Otherwise the original list will be returned.
        /// </summary>
        [EscapeGCop("The condition param should not be last in this case.")]
        public static Task<IEnumerable<TSource>> FilterIf<TSource>(this Task<TSource[]> @this,
             bool condition, Func<TSource, bool> predicate)
            => condition ? @this.ForLinq().Where(predicate) : @this.ForLinq();

        public static Task<bool> HasMany<TSource>(this Task<TSource[]> @this)
            => @this.Get(x => x.HasMany());

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<TSource[]> @this, TSource item)
            => @this.Get(x => x.Except(item));

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<TSource[]> @this,
            IEnumerable<TSource> items)
            => @this.ForLinq().Except(items);

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<TSource[]> @this,
           Task<IEnumerable<TSource>> items)
            => @this.ForLinq().Except(items);

        public static Task<IEnumerable<TSource>> Concat<TSource>(this Task<IEnumerable<TSource>> @this, Task<TSource[]> other)
            => @this.Get(x => x.Concat(other.ForLinq()));

        public static Task<IEnumerable<TSource>> Concat<TSource, TOther>(this Task<TSource[]> @this, TOther other) where TOther : IEnumerable<TSource>
            => @this.Get(x => x.Concat(other));

        public static Task<IEnumerable<TSource>> Concat<TSource, TOther>(this Task<TSource[]> @this, Task<TOther> other) where TOther : IEnumerable<TSource>
            => @this.Get(async x => x.Concat(await other));

        public static Task<IEnumerable<TSource>> Where<TSource>(this Task<TSource[]> @this, Func<TSource, Task<bool>> predicate)
          => @this.Get(x => x.Where(predicate));        
        #endregion

       #region IOrderedEnumerable<TSource>
        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TResult> func)
            => @this.Get(x => x.OrEmpty().Select(func));

        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, Task<TResult>> func)
            => @this.ForLinq().Select(func);

        /// <summary>
        /// Selects the requested items one by one rather than in parallel. Use this in database operations to prevent over-concurrency.
        /// </summary>
        public static Task<IEnumerable<TResult>> SequentialSelect<TSource, TResult>(
            this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, Task<TResult>> selector)
            => @this.ForLinq().SequentialSelect(selector);

        public static Task<IEnumerable<TSource>> WhereAsync<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, Task<bool>> func)
         => @this.ForLinq().WhereAsync(func);

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

        public static Task<IEnumerable<TResult>> OfType<TSource, TResult>(this Task<IOrderedEnumerable<TSource>> @this)
            => @this.ForLinq().OfType<TSource, TResult>();

        public static Task<TSource> WithMin<TSource, TKey>(this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TKey> keySelector)
            => @this.Get(v => v.OrEmpty().WithMin(keySelector));

        public static Task<TSource> WithMax<TSource, TKey>(this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TKey> keySelector)
            => @this.Get(v => v.OrEmpty().WithMax(keySelector));

        public static Task<IEnumerable<TSource>> Concat<TSource>(
          this Task<IOrderedEnumerable<TSource>> @this, IEnumerable<TSource> second)
            => @this.ForLinq().Concat(second);

        public static Task<IEnumerable<TSource>> Distinct<TSource, TResult>(this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TResult> func)
            => @this.ForLinq().Distinct(func);

        public static Task<IEnumerable<TSource>> Distinct<TSource>(this Task<IOrderedEnumerable<TSource>> @this)
            => @this.ForLinq().Distinct();

        public static Task<IEnumerable<IGrouping<TKey, TSource>>> GroupBy<TSource, TKey>(
            this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TKey> groupBy)
            => @this.ForLinq().GroupBy(groupBy);

        public static Task<TSource> First<TSource>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().First(func);

        public static Task<TSource> First<TSource>(this Task<IOrderedEnumerable<TSource>> @this)
            => @this.ForLinq().First();

        public static Task<TSource> FirstOrDefault<TSource>(
          this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().FirstOrDefault(func);

        public static Task<TSource> FirstOrDefault<TSource>(this Task<IOrderedEnumerable<TSource>> @this)
            => @this.ForLinq().FirstOrDefault();

        public static Task<IEnumerable<TSource>> Intersect<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, IEnumerable<TSource> second) => @this.ForLinq().Intersect(second);

        public static Task<TSource> Last<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().Last(func);

        public static Task<TSource> LastOrDefault<TSource>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().LastOrDefault(func);

        public static Task<TSource> Last<TSource>(this Task<IOrderedEnumerable<TSource>> @this)
            => @this.ForLinq().Last();

        public static Task<TSource> LastOrDefault<TSource>(this Task<IOrderedEnumerable<TSource>> @this) 
            => @this.ForLinq().LastOrDefault();

        public static Task<IOrderedEnumerable<TSource>> OrderBy<TSource, TKey>(
        this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, TKey> func) => @this.ForLinq().OrderBy(func);

        public static Task<IOrderedEnumerable<TSource>> OrderByDescending<TSource, TKey>(
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

        public static Task<TSource[]> ToArray<TSource>(this Task<IOrderedEnumerable<TSource>> @this) => @this.ForLinq().ToArray();
        
        public static Task<List<TSource>> ToList<TSource>(this Task<IOrderedEnumerable<TSource>> @this) => @this.ForLinq().ToList();

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

        public static Task<bool> None<TSource>(this Task<IOrderedEnumerable<TSource>> @this)
         => @this.ForLinq().None();

        public static Task<bool> None<TSource>(this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().None(func);

        public static Task<bool> None<TSource>(this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, Task<bool>> func)
            => @this.ForLinq().None(func);

        public static Task<bool> None<TSource>(this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, int, bool> func)
            => @this.ForLinq().None(func);

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

        public static Task<IEnumerable<TSource>> OrEmpty<TSource>(this Task<IOrderedEnumerable<TSource>> @this) 
        => @this.ForLinq().OrEmpty() ?? Task.FromResult(Enumerable.Empty<TSource>());
 
        /// <summary>
        /// If a specified condition is true, then the filter predicate will be executed.
        /// Otherwise the original list will be returned.
        /// </summary>
        [EscapeGCop("The condition param should not be last in this case.")]
        public static Task<IEnumerable<TSource>> FilterIf<TSource>(this Task<IOrderedEnumerable<TSource>> @this,
             bool condition, Func<TSource, bool> predicate)
            => condition ? @this.ForLinq().Where(predicate) : @this.ForLinq();

        public static Task<bool> HasMany<TSource>(this Task<IOrderedEnumerable<TSource>> @this)
            => @this.Get(x => x.HasMany());

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<IOrderedEnumerable<TSource>> @this, TSource item)
            => @this.Get(x => x.Except(item));

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<IOrderedEnumerable<TSource>> @this,
            IEnumerable<TSource> items)
            => @this.ForLinq().Except(items);

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<IOrderedEnumerable<TSource>> @this,
           Task<IEnumerable<TSource>> items)
            => @this.ForLinq().Except(items);

        public static Task<IEnumerable<TSource>> Concat<TSource>(this Task<IEnumerable<TSource>> @this, Task<IOrderedEnumerable<TSource>> other)
            => @this.Get(x => x.Concat(other.ForLinq()));

        public static Task<IEnumerable<TSource>> Concat<TSource, TOther>(this Task<IOrderedEnumerable<TSource>> @this, TOther other) where TOther : IEnumerable<TSource>
            => @this.Get(x => x.Concat(other));

        public static Task<IEnumerable<TSource>> Concat<TSource, TOther>(this Task<IOrderedEnumerable<TSource>> @this, Task<TOther> other) where TOther : IEnumerable<TSource>
            => @this.Get(async x => x.Concat(await other));

        public static Task<IEnumerable<TSource>> Where<TSource>(this Task<IOrderedEnumerable<TSource>> @this, Func<TSource, Task<bool>> predicate)
          => @this.Get(x => x.Where(predicate));        
        #endregion

       #region List<TSource>
        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<List<TSource>> @this, Func<TSource, TResult> func)
            => @this.Get(x => x.OrEmpty().Select(func));

        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<List<TSource>> @this, Func<TSource, Task<TResult>> func)
            => @this.ForLinq().Select(func);

        /// <summary>
        /// Selects the requested items one by one rather than in parallel. Use this in database operations to prevent over-concurrency.
        /// </summary>
        public static Task<IEnumerable<TResult>> SequentialSelect<TSource, TResult>(
            this Task<List<TSource>> @this, Func<TSource, Task<TResult>> selector)
            => @this.ForLinq().SequentialSelect(selector);

        public static Task<IEnumerable<TSource>> WhereAsync<TSource>(
        this Task<List<TSource>> @this, Func<TSource, Task<bool>> func)
         => @this.ForLinq().WhereAsync(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<List<TSource>> @this, Func<TSource, IEnumerable<TResult>> func)
            => @this.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<List<TSource>> @this, Func<TSource, IEnumerable<Task<TResult>>> func)
            => @this.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<List<TSource>> @this, Func<TSource, Task<IEnumerable<TResult>>> func)
            => @this.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TSource>> Except<TSource>(
          this Task<List<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().Except(func);

        public static Task<IEnumerable<TSource>> ExceptNull<TSource>(this Task<List<TSource>> @this)
           => @this.ForLinq().ExceptNull();

        public static Task<IEnumerable<TResult>> Cast<TSource, TResult>(this Task<List<TSource>> @this)
            => @this.ForLinq().Cast<TSource, TResult>();

        public static Task<IEnumerable<TResult>> OfType<TSource, TResult>(this Task<List<TSource>> @this)
            => @this.ForLinq().OfType<TSource, TResult>();

        public static Task<TSource> WithMin<TSource, TKey>(this Task<List<TSource>> @this, Func<TSource, TKey> keySelector)
            => @this.Get(v => v.OrEmpty().WithMin(keySelector));

        public static Task<TSource> WithMax<TSource, TKey>(this Task<List<TSource>> @this, Func<TSource, TKey> keySelector)
            => @this.Get(v => v.OrEmpty().WithMax(keySelector));

        public static Task<IEnumerable<TSource>> Concat<TSource>(
          this Task<List<TSource>> @this, IEnumerable<TSource> second)
            => @this.ForLinq().Concat(second);

        public static Task<IEnumerable<TSource>> Distinct<TSource, TResult>(this Task<List<TSource>> @this, Func<TSource, TResult> func)
            => @this.ForLinq().Distinct(func);

        public static Task<IEnumerable<TSource>> Distinct<TSource>(this Task<List<TSource>> @this)
            => @this.ForLinq().Distinct();

        public static Task<IEnumerable<IGrouping<TKey, TSource>>> GroupBy<TSource, TKey>(
            this Task<List<TSource>> @this, Func<TSource, TKey> groupBy)
            => @this.ForLinq().GroupBy(groupBy);

        public static Task<TSource> First<TSource>(
          this Task<List<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().First(func);

        public static Task<TSource> First<TSource>(this Task<List<TSource>> @this)
            => @this.ForLinq().First();

        public static Task<TSource> FirstOrDefault<TSource>(
          this Task<List<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().FirstOrDefault(func);

        public static Task<TSource> FirstOrDefault<TSource>(this Task<List<TSource>> @this)
            => @this.ForLinq().FirstOrDefault();

        public static Task<IEnumerable<TSource>> Intersect<TSource>(
        this Task<List<TSource>> @this, IEnumerable<TSource> second) => @this.ForLinq().Intersect(second);

        public static Task<TSource> Last<TSource>(
        this Task<List<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().Last(func);

        public static Task<TSource> LastOrDefault<TSource>(
        this Task<List<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().LastOrDefault(func);

        public static Task<TSource> Last<TSource>(this Task<List<TSource>> @this)
            => @this.ForLinq().Last();

        public static Task<TSource> LastOrDefault<TSource>(this Task<List<TSource>> @this) 
            => @this.ForLinq().LastOrDefault();

        public static Task<IOrderedEnumerable<TSource>> OrderBy<TSource, TKey>(
        this Task<List<TSource>> @this, Func<TSource, TKey> func) => @this.ForLinq().OrderBy(func);

        public static Task<IOrderedEnumerable<TSource>> OrderByDescending<TSource, TKey>(
        this Task<List<TSource>> @this, Func<TSource, TKey> func) => @this.ForLinq().OrderByDescending(func);

        public static Task<IEnumerable<TSource>> Reverse<TSource>(
        this Task<List<TSource>> @this) => @this.ForLinq().Reverse();

        public static Task<TSource> Single<TSource>(
        this Task<List<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().Single(func);

        public static Task<TSource> SingleOrDefault<TSource>(
        this Task<List<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().SingleOrDefault(func);

        public static Task<IEnumerable<TSource>> Union<TSource>(
        this Task<List<TSource>> @this, IEnumerable<TSource> second) => @this.ForLinq().Union(second);

        public static Task<IEnumerable<TSource>> Where<TSource>(
        this Task<List<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().Where(func);

        public static Task<IEnumerable<TSource>> Where<TSource>(
        this Task<List<TSource>> @this, Func<TSource, int, bool> func) => @this.ForLinq().Where(func);

        public static Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(
        this Task<List<TSource>> @this, IEnumerable<TSecond> second, Func<TSource, TSecond, TResult> func)
            => @this.ForLinq().Zip(second, func);

        public static Task<IEnumerable<TSource>> Skip<TSource>(
        this Task<List<TSource>> @this, int count) => @this.ForLinq().Skip(count);

        public static Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<List<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().SkipWhile(func);

        public static Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<List<TSource>> @this, Func<TSource, int, bool> func) => @this.ForLinq().SkipWhile(func);

        public static Task<IEnumerable<TSource>> Take<TSource>(
        this Task<List<TSource>> @this, int count) => @this.ForLinq().Take(count);

        public static Task<TSource[]> ToArray<TSource>(this Task<List<TSource>> @this) => @this.ForLinq().ToArray();
        
        public static Task<List<TSource>> ToList<TSource>(this Task<List<TSource>> @this) => @this.ForLinq().ToList();

        public static Task<IEnumerable<TSource>> Take<TSource>(
        this Task<List<TSource>> @this, int lower, int count) => @this.ForLinq().Take(lower, count);

        public static Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<List<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().TakeWhile(func);

        public static Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<List<TSource>> @this, Func<TSource, int, bool> func) => @this.ForLinq().TakeWhile(func);

        public static Task<TSource> Aggregate<TSource>(
        this Task<List<TSource>> @this, Func<TSource, TSource, TSource> func)
            => @this.ForLinq().Aggregate(func);

        public static Task<TAccumulate> Aggregate<TSource, TAccumulate>(
        this Task<List<TSource>> @this, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => @this.ForLinq().Aggregate(seed, func);

        public static Task<TResult> Aggregate<TSource, TAccumulate, TResult>(
        this Task<List<TSource>> @this, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func1, Func<TAccumulate, TResult> func2) => @this.ForLinq().Aggregate(seed, func1, func2);

        public static Task<bool> All<TSource>(this Task<List<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().All(func);

        public static Task<bool> Any<TSource>(this Task<List<TSource>> @this)
            => @this.ForLinq().Any();

        public static Task<bool> Any<TSource>(this Task<List<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().Any(func);

        public static Task<bool> Any<TSource>(this Task<List<TSource>> @this, Func<TSource, Task<bool>> func)
            => @this.ForLinq().Any(func);

        public static Task<bool> Any<TSource>(this Task<List<TSource>> @this, Func<TSource, int, bool> func)
            => @this.ForLinq().Any(func);

        public static Task<bool> None<TSource>(this Task<List<TSource>> @this)
         => @this.ForLinq().None();

        public static Task<bool> None<TSource>(this Task<List<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().None(func);

        public static Task<bool> None<TSource>(this Task<List<TSource>> @this, Func<TSource, Task<bool>> func)
            => @this.ForLinq().None(func);

        public static Task<bool> None<TSource>(this Task<List<TSource>> @this, Func<TSource, int, bool> func)
            => @this.ForLinq().None(func);

        public static Task<bool> Contains<TSource>(this Task<List<TSource>> @this, TSource item)
            => @this.ForLinq().Contains(item);

        public static Task<bool> Contains<TSource>(this Task<List<TSource>> @this, Task<TSource> item)
            => @this.ForLinq().Contains(item);

        public static Task<decimal> Average<TSource>(this Task<List<TSource>> @this, Func<TSource, decimal> func)
            => @this.ForLinq().Average(func);

        public static Task<int> Count<TSource>(this Task<List<TSource>> @this) => @this.ForLinq().Count();

        public static Task<int> Count<TSource>(
        this Task<List<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().Count(func);

        public static Task<int> Count<TSource>(
        this Task<List<TSource>> @this, Func<TSource, int, bool> func) => @this.ForLinq().Count(func);

        public static Task<decimal> Sum<TSource>(
        this Task<List<TSource>> @this, Func<TSource, decimal> func) => @this.ForLinq().Sum(func);

        public static Task<TimeSpan> Sum<TSource>(
        this Task<List<TSource>> @this, Func<TSource, TimeSpan> func) => @this.ForLinq().Sum(func);

        public static Task<TResult> Max<TSource, TResult>(
        this Task<List<TSource>> @this, Func<TSource, TResult> func) => @this.ForLinq().Max(func);

        public static Task<TResult> Min<TSource, TResult>(
        this Task<List<TSource>> @this, Func<TSource, TResult> func) => @this.ForLinq().Min(func);

        public static Task<IEnumerable<TSource>> OrEmpty<TSource>(this Task<List<TSource>> @this) 
        => @this.ForLinq().OrEmpty() ?? Task.FromResult(Enumerable.Empty<TSource>());
 
        /// <summary>
        /// If a specified condition is true, then the filter predicate will be executed.
        /// Otherwise the original list will be returned.
        /// </summary>
        [EscapeGCop("The condition param should not be last in this case.")]
        public static Task<IEnumerable<TSource>> FilterIf<TSource>(this Task<List<TSource>> @this,
             bool condition, Func<TSource, bool> predicate)
            => condition ? @this.ForLinq().Where(predicate) : @this.ForLinq();

        public static Task<bool> HasMany<TSource>(this Task<List<TSource>> @this)
            => @this.Get(x => x.HasMany());

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<List<TSource>> @this, TSource item)
            => @this.Get(x => x.Except(item));

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<List<TSource>> @this,
            IEnumerable<TSource> items)
            => @this.ForLinq().Except(items);

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<List<TSource>> @this,
           Task<IEnumerable<TSource>> items)
            => @this.ForLinq().Except(items);

        public static Task<IEnumerable<TSource>> Concat<TSource>(this Task<IEnumerable<TSource>> @this, Task<List<TSource>> other)
            => @this.Get(x => x.Concat(other.ForLinq()));

        public static Task<IEnumerable<TSource>> Concat<TSource, TOther>(this Task<List<TSource>> @this, TOther other) where TOther : IEnumerable<TSource>
            => @this.Get(x => x.Concat(other));

        public static Task<IEnumerable<TSource>> Concat<TSource, TOther>(this Task<List<TSource>> @this, Task<TOther> other) where TOther : IEnumerable<TSource>
            => @this.Get(async x => x.Concat(await other));

        public static Task<IEnumerable<TSource>> Where<TSource>(this Task<List<TSource>> @this, Func<TSource, Task<bool>> predicate)
          => @this.Get(x => x.Where(predicate));        
        #endregion

       #region IList<TSource>
        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IList<TSource>> @this, Func<TSource, TResult> func)
            => @this.Get(x => x.OrEmpty().Select(func));

        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        public static Task<IEnumerable<TResult>> Select<TSource, TResult>(
          this Task<IList<TSource>> @this, Func<TSource, Task<TResult>> func)
            => @this.ForLinq().Select(func);

        /// <summary>
        /// Selects the requested items one by one rather than in parallel. Use this in database operations to prevent over-concurrency.
        /// </summary>
        public static Task<IEnumerable<TResult>> SequentialSelect<TSource, TResult>(
            this Task<IList<TSource>> @this, Func<TSource, Task<TResult>> selector)
            => @this.ForLinq().SequentialSelect(selector);

        public static Task<IEnumerable<TSource>> WhereAsync<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, Task<bool>> func)
         => @this.ForLinq().WhereAsync(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IList<TSource>> @this, Func<TSource, IEnumerable<TResult>> func)
            => @this.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IList<TSource>> @this, Func<TSource, IEnumerable<Task<TResult>>> func)
            => @this.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TResult>> SelectMany<TSource, TResult>(
          this Task<IList<TSource>> @this, Func<TSource, Task<IEnumerable<TResult>>> func)
            => @this.ForLinq().SelectMany(func);

        public static Task<IEnumerable<TSource>> Except<TSource>(
          this Task<IList<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().Except(func);

        public static Task<IEnumerable<TSource>> ExceptNull<TSource>(this Task<IList<TSource>> @this)
           => @this.ForLinq().ExceptNull();

        public static Task<IEnumerable<TResult>> Cast<TSource, TResult>(this Task<IList<TSource>> @this)
            => @this.ForLinq().Cast<TSource, TResult>();

        public static Task<IEnumerable<TResult>> OfType<TSource, TResult>(this Task<IList<TSource>> @this)
            => @this.ForLinq().OfType<TSource, TResult>();

        public static Task<TSource> WithMin<TSource, TKey>(this Task<IList<TSource>> @this, Func<TSource, TKey> keySelector)
            => @this.Get(v => v.OrEmpty().WithMin(keySelector));

        public static Task<TSource> WithMax<TSource, TKey>(this Task<IList<TSource>> @this, Func<TSource, TKey> keySelector)
            => @this.Get(v => v.OrEmpty().WithMax(keySelector));

        public static Task<IEnumerable<TSource>> Concat<TSource>(
          this Task<IList<TSource>> @this, IEnumerable<TSource> second)
            => @this.ForLinq().Concat(second);

        public static Task<IEnumerable<TSource>> Distinct<TSource, TResult>(this Task<IList<TSource>> @this, Func<TSource, TResult> func)
            => @this.ForLinq().Distinct(func);

        public static Task<IEnumerable<TSource>> Distinct<TSource>(this Task<IList<TSource>> @this)
            => @this.ForLinq().Distinct();

        public static Task<IEnumerable<IGrouping<TKey, TSource>>> GroupBy<TSource, TKey>(
            this Task<IList<TSource>> @this, Func<TSource, TKey> groupBy)
            => @this.ForLinq().GroupBy(groupBy);

        public static Task<TSource> First<TSource>(
          this Task<IList<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().First(func);

        public static Task<TSource> First<TSource>(this Task<IList<TSource>> @this)
            => @this.ForLinq().First();

        public static Task<TSource> FirstOrDefault<TSource>(
          this Task<IList<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().FirstOrDefault(func);

        public static Task<TSource> FirstOrDefault<TSource>(this Task<IList<TSource>> @this)
            => @this.ForLinq().FirstOrDefault();

        public static Task<IEnumerable<TSource>> Intersect<TSource>(
        this Task<IList<TSource>> @this, IEnumerable<TSource> second) => @this.ForLinq().Intersect(second);

        public static Task<TSource> Last<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().Last(func);

        public static Task<TSource> LastOrDefault<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().LastOrDefault(func);

        public static Task<TSource> Last<TSource>(this Task<IList<TSource>> @this)
            => @this.ForLinq().Last();

        public static Task<TSource> LastOrDefault<TSource>(this Task<IList<TSource>> @this) 
            => @this.ForLinq().LastOrDefault();

        public static Task<IOrderedEnumerable<TSource>> OrderBy<TSource, TKey>(
        this Task<IList<TSource>> @this, Func<TSource, TKey> func) => @this.ForLinq().OrderBy(func);

        public static Task<IOrderedEnumerable<TSource>> OrderByDescending<TSource, TKey>(
        this Task<IList<TSource>> @this, Func<TSource, TKey> func) => @this.ForLinq().OrderByDescending(func);

        public static Task<IEnumerable<TSource>> Reverse<TSource>(
        this Task<IList<TSource>> @this) => @this.ForLinq().Reverse();

        public static Task<TSource> Single<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().Single(func);

        public static Task<TSource> SingleOrDefault<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().SingleOrDefault(func);

        public static Task<IEnumerable<TSource>> Union<TSource>(
        this Task<IList<TSource>> @this, IEnumerable<TSource> second) => @this.ForLinq().Union(second);

        public static Task<IEnumerable<TSource>> Where<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().Where(func);

        public static Task<IEnumerable<TSource>> Where<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, int, bool> func) => @this.ForLinq().Where(func);

        public static Task<IEnumerable<TResult>> Zip<TSource, TSecond, TResult>(
        this Task<IList<TSource>> @this, IEnumerable<TSecond> second, Func<TSource, TSecond, TResult> func)
            => @this.ForLinq().Zip(second, func);

        public static Task<IEnumerable<TSource>> Skip<TSource>(
        this Task<IList<TSource>> @this, int count) => @this.ForLinq().Skip(count);

        public static Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().SkipWhile(func);

        public static Task<IEnumerable<TSource>> SkipWhile<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, int, bool> func) => @this.ForLinq().SkipWhile(func);

        public static Task<IEnumerable<TSource>> Take<TSource>(
        this Task<IList<TSource>> @this, int count) => @this.ForLinq().Take(count);

        public static Task<TSource[]> ToArray<TSource>(this Task<IList<TSource>> @this) => @this.ForLinq().ToArray();
        
        public static Task<List<TSource>> ToList<TSource>(this Task<IList<TSource>> @this) => @this.ForLinq().ToList();

        public static Task<IEnumerable<TSource>> Take<TSource>(
        this Task<IList<TSource>> @this, int lower, int count) => @this.ForLinq().Take(lower, count);

        public static Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().TakeWhile(func);

        public static Task<IEnumerable<TSource>> TakeWhile<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, int, bool> func) => @this.ForLinq().TakeWhile(func);

        public static Task<TSource> Aggregate<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, TSource, TSource> func)
            => @this.ForLinq().Aggregate(func);

        public static Task<TAccumulate> Aggregate<TSource, TAccumulate>(
        this Task<IList<TSource>> @this, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            => @this.ForLinq().Aggregate(seed, func);

        public static Task<TResult> Aggregate<TSource, TAccumulate, TResult>(
        this Task<IList<TSource>> @this, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func1, Func<TAccumulate, TResult> func2) => @this.ForLinq().Aggregate(seed, func1, func2);

        public static Task<bool> All<TSource>(this Task<IList<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().All(func);

        public static Task<bool> Any<TSource>(this Task<IList<TSource>> @this)
            => @this.ForLinq().Any();

        public static Task<bool> Any<TSource>(this Task<IList<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().Any(func);

        public static Task<bool> Any<TSource>(this Task<IList<TSource>> @this, Func<TSource, Task<bool>> func)
            => @this.ForLinq().Any(func);

        public static Task<bool> Any<TSource>(this Task<IList<TSource>> @this, Func<TSource, int, bool> func)
            => @this.ForLinq().Any(func);

        public static Task<bool> None<TSource>(this Task<IList<TSource>> @this)
         => @this.ForLinq().None();

        public static Task<bool> None<TSource>(this Task<IList<TSource>> @this, Func<TSource, bool> func)
            => @this.ForLinq().None(func);

        public static Task<bool> None<TSource>(this Task<IList<TSource>> @this, Func<TSource, Task<bool>> func)
            => @this.ForLinq().None(func);

        public static Task<bool> None<TSource>(this Task<IList<TSource>> @this, Func<TSource, int, bool> func)
            => @this.ForLinq().None(func);

        public static Task<bool> Contains<TSource>(this Task<IList<TSource>> @this, TSource item)
            => @this.ForLinq().Contains(item);

        public static Task<bool> Contains<TSource>(this Task<IList<TSource>> @this, Task<TSource> item)
            => @this.ForLinq().Contains(item);

        public static Task<decimal> Average<TSource>(this Task<IList<TSource>> @this, Func<TSource, decimal> func)
            => @this.ForLinq().Average(func);

        public static Task<int> Count<TSource>(this Task<IList<TSource>> @this) => @this.ForLinq().Count();

        public static Task<int> Count<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, bool> func) => @this.ForLinq().Count(func);

        public static Task<int> Count<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, int, bool> func) => @this.ForLinq().Count(func);

        public static Task<decimal> Sum<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, decimal> func) => @this.ForLinq().Sum(func);

        public static Task<TimeSpan> Sum<TSource>(
        this Task<IList<TSource>> @this, Func<TSource, TimeSpan> func) => @this.ForLinq().Sum(func);

        public static Task<TResult> Max<TSource, TResult>(
        this Task<IList<TSource>> @this, Func<TSource, TResult> func) => @this.ForLinq().Max(func);

        public static Task<TResult> Min<TSource, TResult>(
        this Task<IList<TSource>> @this, Func<TSource, TResult> func) => @this.ForLinq().Min(func);

        public static Task<IEnumerable<TSource>> OrEmpty<TSource>(this Task<IList<TSource>> @this) 
        => @this.ForLinq().OrEmpty() ?? Task.FromResult(Enumerable.Empty<TSource>());
 
        /// <summary>
        /// If a specified condition is true, then the filter predicate will be executed.
        /// Otherwise the original list will be returned.
        /// </summary>
        [EscapeGCop("The condition param should not be last in this case.")]
        public static Task<IEnumerable<TSource>> FilterIf<TSource>(this Task<IList<TSource>> @this,
             bool condition, Func<TSource, bool> predicate)
            => condition ? @this.ForLinq().Where(predicate) : @this.ForLinq();

        public static Task<bool> HasMany<TSource>(this Task<IList<TSource>> @this)
            => @this.Get(x => x.HasMany());

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<IList<TSource>> @this, TSource item)
            => @this.Get(x => x.Except(item));

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<IList<TSource>> @this,
            IEnumerable<TSource> items)
            => @this.ForLinq().Except(items);

        public static Task<IEnumerable<TSource>> Except<TSource>(this Task<IList<TSource>> @this,
           Task<IEnumerable<TSource>> items)
            => @this.ForLinq().Except(items);

        public static Task<IEnumerable<TSource>> Concat<TSource>(this Task<IEnumerable<TSource>> @this, Task<IList<TSource>> other)
            => @this.Get(x => x.Concat(other.ForLinq()));

        public static Task<IEnumerable<TSource>> Concat<TSource, TOther>(this Task<IList<TSource>> @this, TOther other) where TOther : IEnumerable<TSource>
            => @this.Get(x => x.Concat(other));

        public static Task<IEnumerable<TSource>> Concat<TSource, TOther>(this Task<IList<TSource>> @this, Task<TOther> other) where TOther : IEnumerable<TSource>
            => @this.Get(async x => x.Concat(await other));

        public static Task<IEnumerable<TSource>> Where<TSource>(this Task<IList<TSource>> @this, Func<TSource, Task<bool>> predicate)
          => @this.Get(x => x.Where(predicate));        
        #endregion

       }
}