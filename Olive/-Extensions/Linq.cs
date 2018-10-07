﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olive
{
#pragma warning disable GCop112 // This class is too large. Break its responsibilities down into more classes.
    partial class OliveExtensions
#pragma warning restore GCop112 // This class is too large. Break its responsibilities down into more classes.
    {
        static Random RandomProvider = new Random(LocalTime.Now.TimeOfDay.Milliseconds);

        public static string ToString(this IEnumerable @this, string seperator)
        {
            if (@this == null) return "{NULL}";
            return ToString(@this.Cast<object>(), seperator);
        }

        public static Task<string> ToString(this Task<IEnumerable> @this, string seperator)
        {
            if (@this == null) return Task.FromResult("{NULL}");
            return @this.Get(x => x.ToString(seperator));
        }

        public static Task<string> ToString<T>(this Task<IEnumerable<T>> @this, string seperator)
        {
            if (@this == null) return Task.FromResult("{NULL}");
            return @this.Get(x => x.ToString(seperator));
        }

        public static string ToFormatString<T>(this IEnumerable<T> @this, string format, string seperator, string lastSeperator) =>
            @this.Select(i => format.FormatWith(i)).ToString(seperator, lastSeperator);

        public static bool Any<T>(this IEnumerable<T> @this, Func<T, int, bool> predicate) => @this.Any(predicate);

        [EscapeGCop("I am the solution to this GCop warning")]
        public static bool None<T>(this IEnumerable<T> @this, Func<T, int, bool> predicate) => !@this.Any(predicate);

        public static string ToFormatString<T>(this IEnumerable<T> @this, string format, string seperator)
            => @this.Select(i => format.FormatWith(i)).ToString(seperator);

        public static string ToString<T>(this IEnumerable<T> @this, string seperator)
            => ToString(@this, seperator, seperator);

        public static string ToString<T>(this IEnumerable<T> @this, string seperator, string lastSeperator)
        {
            var result = new StringBuilder();

            var items = @this.Cast<object>().ToArray();

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];

                if (item == null) result.Append("{NULL}");
                else result.Append(item.ToString());

                if (i < items.Length - 2)
                    result.Append(seperator);

                if (i == items.Length - 2)
                    result.Append(lastSeperator);
            }

            return result.ToString();
        }

        public static int IndexOf<T>(this IEnumerable<T> @this, T element)
        {
            if (@this == null)
                throw new NullReferenceException("No collection is given for the extension method IndexOf().");

            if (@this.Contains(element) == false) return -1;

            var result = 0;
            foreach (var el in @this)
            {
                if (el == null)
                {
                    if (element == null) return result;
                    else continue;
                }

                if (el.Equals(element)) return result;
                result++;
            }

            return -1;
        }

        /// <summary>
        /// Gets the index of the first item in this list which matches the specified criteria.
        /// </summary>
        public static int IndexOf<T>(this IEnumerable<T> @this, Func<T, bool> criteria)
        {
            var result = 0;

            foreach (var item in @this)
            {
                if (criteria(item)) return result;

                result++;
            }

            return -1;
        }

        public static void RemoveWhere<T>(this IList<T> @this, Func<T, bool> selector)
        {
            lock (@this)
            {
                var itemsToRemove = @this.Where(selector).ToList();
                @this.Remove(itemsToRemove);
            }
        }

        /// <summary>
        /// Gets all items of this list except those meeting a specified criteria.
        /// </summary>
        /// <param name="criteria">Exclusion criteria</param>
        [EscapeGCop("It is the Except definition and so it cannot call itself")]
        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, Func<T, bool> criteria) => list.Where(i => !criteria(i));

        public static IEnumerable<T> Except<T>(this IEnumerable<T> @this, T item) => @this.Except(new T[] { item });

        [EscapeGCop("It is the Except definition and so it cannot call itself")]
        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, params T[] items)
        {
            if (items == null) return list;

            return list.Where(x => !items.Contains(x));
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> @this, List<T> itemsToExclude) => @this.Except(itemsToExclude.ToArray());

        public static IEnumerable<char> Except(this IEnumerable<char> @this, IEnumerable<char> itemsToExclude) =>
            @this.Except(itemsToExclude.ToArray());

        public static IEnumerable<T> Except<T>(this IEnumerable<T> @this, IEnumerable<T> itemsToExclude, bool alsoDistinct = false)
        {
            var result = @this.Except(itemsToExclude.ToArray());

            if (alsoDistinct) result = result.Distinct();

            return result;
        }

        public static IEnumerable<string> Except(this IEnumerable<string> @this, IEnumerable<string> itemsToExclude) =>
            @this.Except(itemsToExclude.ToArray());

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<T> ExceptNull<T>(this IEnumerable<T> @this) where T : class => @this.Where(i => i != null);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<int> ExceptNull(this IEnumerable<int?> @this) =>
            @this.Where(i => i.HasValue).Select(x => x.Value);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<double> ExceptNull(this IEnumerable<double?> @this) =>
            @this.Where(i => i.HasValue).Select(x => x.Value);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<TimeSpan> ExceptNull(this IEnumerable<TimeSpan?> @this) =>
            @this.Where(i => i.HasValue).Select(x => x.Value);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<decimal> ExceptNull(this IEnumerable<decimal?> @this) =>
            @this.Where(i => i.HasValue).Select(x => x.Value);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<bool> ExceptNull(this IEnumerable<bool?> @this) => @this.Where(i => i.HasValue).Select(x => x.Value);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<DateTime> ExceptNull(this IEnumerable<DateTime?> @this) =>
            @this.Where(i => i.HasValue).Select(x => x.Value);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<Guid> ExceptNull(this IEnumerable<Guid?> @this) => @this.Where(i => i.HasValue).Select(x => x.Value);

        public static bool IsSingle<T>(this IEnumerable<T> @this) => IsSingle<T>(@this, x => true);

        public static bool IsSingle<T>(this IEnumerable<T> @this, Func<T, bool> criteria)
        {
            var visitedAny = false;

            foreach (var item in @this.Where(criteria))
            {
                if (visitedAny) return false;
                visitedAny = true;
            }

            return visitedAny;
        }

        [EscapeGCop("This is for performance reasons.")]
        public static bool IsSingle<T>(this IEnumerable<T> list, out T first) where T : class
        {
            first = null;

            foreach (var item in list)
                if (first == null) first = item;
                else return false;

            return first != null;
        }

        [EscapeGCop("This is impossible to instantiate an interface so, we cannot use IEnumerable.")]
        public static List<T> Clone<T>(this List<T> list)
        {
            if (list == null) return list;
            return new List<T>(list);
        }

        /// <summary>
        /// Adds the specified list to the beginning of this list.
        /// </summary>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> @this, IEnumerable<T> prefix) => prefix.Concat(@this);

        /// <summary>
        /// Adds the specified item(s) to the beginning of this list.
        /// </summary>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> @this, params T[] prefix) => prefix.Concat(@this);

        /// <summary>
        /// Performs an action for all items within the list.
        /// </summary>
        public static void Do<T>(this IEnumerable<T> @this, ItemHandler<T> action)
        {
            if (@this == null) return;

            foreach (var item in @this)
                action?.Invoke(item);
        }

        /// <summary>
        /// Performs an action for all items within the list.
        /// </summary>
        public static async Task Do<T>(this IEnumerable<T> @this, Func<T, Task> func)
        {
            if (@this == null || func == null) return;

            foreach (var item in @this) await func(item);
        }

        /// <summary>
        /// Performs an action for all items within the list.
        /// It will provide the index of the item in the list to the action handler as well.
        /// </summary>        
        public static void Do<T>(this IEnumerable<T> @this, Action<T, int> action)
        {
            if (@this == null || action == null) return;

            var index = 0;

            foreach (var item in @this)
            {
                action(item, index);
                index++;
            }
        }

        /// <summary>
        /// Performs an action for all items within the list.
        /// It will provide the index of the item in the list to the action handler as well.
        /// </summary>        
        public static async Task DoAsync<T>(this IEnumerable<T> @this, Func<T, int, Task> action)
        {
            if (@this == null || action == null) return;

            var index = 0;

            foreach (var item in @this)
            {
                await action(item, index);
                index++;
            }
        }

        public delegate void ItemHandler<in T>(T arg);

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> @this, T item) => @this.Concat(new T[] { item });

        public static void AddRange<T>(this IList<T> @this, IEnumerable<T> items)
        {
            foreach (var item in items)
                @this.Add(item);
        }

        /// <summary>
        /// Gets the minimum value of a specified expression in this list. If the list is empty, then the default value of the expression will be returned.
        /// </summary>
        public static R MinOrDefault<T, R>(this IEnumerable<T> @this, Func<T, R> expression)
        {
            if (@this.None()) return default(R);
            return @this.Min(expression);
        }

        /// <summary>
        /// Gets the maximum value of a specified expression in this list. If the list is empty, then the default value of the expression will be returned.
        /// </summary>
        public static R MaxOrDefault<T, R>(this IEnumerable<T> @this, Func<T, R> expression)
        {
            if (@this.None()) return default(R);
            return @this.Max(expression);
        }

        /// <summary>
        /// Gets the maximum value of the specified expression in this list. 
        /// If no items exist in the list then null will be returned. 
        /// </summary>     
        public static R? MaxOrNull<T, R>(this IEnumerable<T> @this, Func<T, R?> expression) where R : struct
        {
            if (@this.None()) return default(R?);
            return @this.Max(expression);
        }

        /// <summary>
        /// Gets the maximum value of the specified expression in this list. 
        /// If no items exist in the list then null will be returned. 
        /// </summary>     
        public static R? MaxOrNull<T, R>(this IEnumerable<T> @this, Func<T, R> expression) where R : struct =>
            @this.MaxOrNull(item => (R?)expression(item));

        /// <summary>
        /// Gets the minimum value of the specified expression in this list. 
        /// If no items exist in the list then null will be returned. 
        /// </summary>     
        public static R? MinOrNull<T, R>(this IEnumerable<T> @this, Func<T, R?> expression) where R : struct
        {
            if (@this.None()) return default(R?);
            return @this.Min(expression);
        }

        /// <summary>
        /// Gets the minimum value of the specified expression in this list. 
        /// If no items exist in the list then null will be returned. 
        /// </summary>     
        public static R? MinOrNull<T, R>(this IEnumerable<T> @this, Func<T, R> expression) where R : struct =>
            @this.MinOrNull(item => (R?)expression(item));

        public static bool IsSubsetOf<T>(this IEnumerable<T> @this, IEnumerable<T> target) => target.ContainsAll(@this);

        /// <summary>
        /// Determines whether this list is equivalent to another specified list. Items in the list should be distinct for accurate result.
        /// </summary>
        public static bool IsEquivalentTo<T>(this IEnumerable<T> @this, IEnumerable<T> other)
        {
            if (@this == null) @this = new T[0];
            if (other == null) other = new T[0];

            if (@this.Count() != other.Count()) return false;

            foreach (var item in @this)
                if (!other.Contains(item)) return false;
            return true;
        }

        /// <summary>
        /// Counts the number of items in this list matching the specified criteria.
        /// </summary>
        public static int Count<T>(this IEnumerable<T> @this, Func<T, int, bool> criteria) => @this.Count((x, i) => criteria(x, i));

        /// <summary>
        /// Picks an item from the list.
        /// </summary>
        public static T PickRandom<T>(this IEnumerable<T> @this)
        {
            if (@this.Any())
            {
                var index = RandomProvider.Next(@this.Count());
                return @this.ElementAt(index);
            }
            else return default(T);
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> @this, int number)
        {
            if (number < 1) throw new ArgumentException("number should be greater than 0.");

            var items = @this as List<T> ?? @this.ToList();

            if (number >= items.Count) number = items.Count;

            var randomIndices = RandomProvider.PickNumbers(number, 0, items.Count - 1);

            foreach (var index in randomIndices)
                yield return items[index];
        }

        /// <summary>
        /// Works as opposite of Contains().
        /// </summary>        
        public static bool Lacks<T>(this IEnumerable<T> @this, T item) => !@this.Contains(item);

        /// <summary>
        /// Determines if this list lacks any item in the specified list.
        /// </summary>        
        public static bool LacksAny<T>(this IEnumerable<T> @this, IEnumerable<T> items) => !@this.ContainsAll(items);

        /// <summary>
        /// Determines if this list lacks all items in the specified list.
        /// </summary>        
        public static bool LacksAll<T>(this IEnumerable<T> @this, IEnumerable<T> items) => !@this.ContainsAny(items.ToArray());

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> @this)
        {
            if (@this.None()) return new T[0];

            var items = @this.ToList();

            return PickRandom(items, items.Count);
        }

        /// <summary>
        /// Returns a subset of the items in this collection.
        /// </summary>
        public static IEnumerable<T> Take<T>(this IEnumerable<T> @this, int lowerBound, int count)
        {
            if (lowerBound < 0) throw new ArgumentOutOfRangeException(nameof(lowerBound));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return Enumerable.Empty<T>();

            return @this.Skip(lowerBound).Take(count);
        }

        public static IEnumerable<T> Distinct<T, TResult>(this IEnumerable<T> @this, Func<T, TResult> selector)
        {
            var keys = new List<TResult>();

            foreach (var item in @this)
            {
                var keyValue = selector(item);

                if (keys.Contains(keyValue)) continue;

                keys.Add(keyValue);
                yield return item;
            }
        }

        /// <summary>
        /// Determines of this list contains all items of another given list.
        /// </summary>        
        public static bool ContainsAll<T>(this IEnumerable<T> @this, IEnumerable<T> items) =>
            items.All(i => @this.Contains(i));

        /// <summary>
        /// Determines if this list contains any of the specified items.
        /// </summary>
        public static bool ContainsAny<T>(this IEnumerable<T> @this, params T[] items) => @this.Intersects(items);

        /// <summary>
        /// Determines if none of the items in this list meet a given criteria.
        /// </summary>
        [EscapeGCop("I am the solution to this GCop warning")]
        public static bool None<T>(this IEnumerable<T> @this, Func<T, bool> criteria) => !@this.Any(criteria);

        /// <summary>
        /// A null safe alternative to Any(). If the source is null it will return false instead of throwing an exception.
        /// </summary>
        public static bool HasAny<TSource>(this IEnumerable<TSource> @this)
           => @this != null && @this.Any();

        /// <summary>
        /// Determines if this is null or an empty list.
        /// </summary>
        public static bool None<T>(this IEnumerable<T> @this) => !@this.HasAny();

        /// <summary>
        /// Determines if this list intersects with another specified list.
        /// </summary>
        public static bool Intersects<T>(this IEnumerable<T> @this, IEnumerable<T> otherList)
        {
            var countList = (@this as ICollection)?.Count;
            var countOther = (otherList as ICollection)?.Count;

            if (countList == null || countOther == null || countOther < countList)
            {
                foreach (var item in otherList)
                    if (@this.Contains(item)) return true;
            }
            else
            {
                foreach (var item in @this)
                    if (otherList.Contains(item)) return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if this list intersects with another specified list.
        /// </summary>
        public static bool Intersects<T>(this IEnumerable<T> @this, params T[] items) => @this.Intersects((IEnumerable<T>)items);

        /// <summary>
        /// Selects the item with maximum of the specified value.
        /// If this list is empty, NULL (or default of T) will be returned.
        /// </summary>
        public static T WithMax<T, TKey>(this IEnumerable<T> @this, Func<T, TKey> keySelector)
        {
            if (@this.None()) return default(T);
            return @this.Aggregate((a, b) => Comparer.Default.Compare(keySelector(a), keySelector(b)) > 0 ? a : b);
        }

        /// <summary>
        /// Selects the item with minimum of the specified value.
        /// </summary>
        public static T WithMin<T, TKey>(this IEnumerable<T> @this, Func<T, TKey> keySelector)
        {
            if (@this.None()) return default(T);
            return @this.Aggregate((a, b) => Comparer.Default.Compare(keySelector(a), keySelector(b)) < 0 ? a : b);
        }

        /// <summary>
        /// Gets the element after a specified item in this list.
        /// If the specified element does not exist in this list, an ArgumentException will be thrown.
        /// If the specified element is the last in the list, NULL will be returned.
        /// </summary>        
        public static T GetElementAfter<T>(this IEnumerable<T> @this, T item) where T : class
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var index = @this.IndexOf(item);
            if (index == -1)
                throw new ArgumentException("The specified item does not exist to this list.");

            if (index == @this.Count() - 1) return null;

            return @this.ElementAt(index + 1);
        }

        /// <summary>
        /// Gets the element before a specified item in this list.
        /// If the specified element does not exist in this list, an ArgumentException will be thrown.
        /// If the specified element is the first in the list, NULL will be returned.
        /// </summary>        
        public static T GetElementBefore<T>(this IEnumerable<T> @this, T item) where T : class
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var index = @this.IndexOf(item);
            if (index == -1)
                throw new ArgumentException("The specified item does not exist to this list.");

            if (index == 0) return null;

            return @this.ElementAt(index - 1);
        }

        public static void AddFormat(this IList<string> @this, string format, params object[] arguments) =>
            @this.Add(string.Format(format, arguments));

        public static void AddFormattedLine(this IList<string> @this, string format, params object[] arguments) =>
            @this.Add(string.Format(format + Environment.NewLine, arguments));

        public static void AddLine(this IList<string> @this, string text) => @this.Add(text + Environment.NewLine);

        /// <summary>
        /// Removes a list of items from this list.
        /// </summary>
        public static void Remove<T>(this IList<T> @this, IEnumerable<T> itemsToRemove)
        {
            if (itemsToRemove != null)
            {
                foreach (var item in itemsToRemove)
                    if (@this.Contains(item)) @this.Remove(item);
            }
        }

        /// <summary>
        /// Determines if all items in this collection are unique.
        /// </summary>
        public static bool AreItemsUnique<T>(this IEnumerable<T> @this)
        {
            if (@this.None()) return true;

            return @this.Distinct().Count() == @this.Count();
        }

        /// <summary>
        /// Returns the union of this list with the specified other lists.
        /// </summary>

        public static IEnumerable<T> Union<T>(this IEnumerable<T> @this, params IEnumerable<T>[] otherLists)
        {
            var result = @this;

            foreach (var other in otherLists)
                result = Enumerable.Union(result, other);

            return result;
        }

        /// <summary>
        /// Returns the union of this list with the specified items.
        /// </summary>
        public static IEnumerable<T> Union<T>(this IEnumerable<T> @this, params T[] otherItems) => @this.Union((IEnumerable<T>)otherItems);

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> @this, params IEnumerable<T>[] otherLists)
        {
            var result = @this;

            foreach (var other in otherLists) result = Enumerable.Concat(result, other);

            return result;
        }

        /// <summary>
        /// Gets the average of the specified expression on all items of this list.
        /// If the list is empty, null will be returned.
        /// </summary>
        public static double? AverageOrDefault<T>(this IEnumerable<T> @this, Func<T, int> selector)
        {
            if (@this.None()) return null;
            else return @this.Average(selector);
        }

        /// <summary>
        /// Gets the average of the specified expression on all items of this list.
        /// If the list is empty, null will be returned.
        /// </summary>
        public static double? AverageOrDefault<T>(this IEnumerable<T> @this, Func<T, int?> selector)
        {
            if (@this.None()) return null;
            else return @this.Average(selector);
        }

        /// <summary>
        /// Gets the average of the specified expression on all items of this list.
        /// If the list is empty, null will be returned.
        /// </summary>
        public static double? AverageOrDefault<T>(this IEnumerable<T> @this, Func<T, double> selector)
        {
            if (@this.None()) return null;
            else return @this.Average(selector);
        }

        /// <summary>
        /// Gets the average of the specified expression on all items of this list.
        /// If the list is empty, null will be returned.
        /// </summary>
        public static double? AverageOrDefault<T>(this IEnumerable<T> @this, Func<T, double?> selector)
        {
            if (@this.None()) return null;
            else return @this.Average(selector);
        }

        /// <summary>
        /// Gets the average of the specified expression on all items of this list.
        /// If the list is empty, null will be returned.
        /// </summary>
        public static decimal? AverageOrDefault<T>(this IEnumerable<T> @this, Func<T, decimal> selector)
        {
            if (@this.None()) return null;
            else return @this.Average(selector);
        }

        /// <summary>
        /// Gets the average of the specified expression on all items of this list.
        /// If the list is empty, null will be returned.
        /// </summary>
        public static decimal? AverageOrDefault<T>(this IEnumerable<T> @this, Func<T, decimal?> selector)
        {
            if (@this.None()) return null;
            else return @this.Average(selector);
        }

        /// <summary>
        /// Trims all elements in this list and excludes all null and "empty string" elements from the list.
        /// </summary>
        public static IEnumerable<string> Trim(this IEnumerable<string> @this)
        {
            if (@this == null) return Enumerable.Empty<string>();

            return @this.Except(v => string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Where(v => v.HasValue()).ToArray();
        }

        /// <summary>
        /// Determines whether this list of strings contains the specified string.
        /// </summary>
        public static bool Contains(this IEnumerable<string> @this, string instance, bool caseSensitive)
        {
            if (caseSensitive || instance.IsEmpty())
                return @this.Contains(instance);
            else return @this.Any(i => i.HasValue() && i.Equals(instance, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines whether this list of strings contains the specified string.
        /// </summary>
        public static bool Lacks(this IEnumerable<string> @this, string instance, bool caseSensitive) =>
            !Contains(@this, instance, caseSensitive);

        /// <summary>
        /// Concats all elements in this list with Environment.NewLine.
        /// </summary>
        public static string ToLinesString<T>(this IEnumerable<T> @this) => @this.ToString(Environment.NewLine);

        /// <summary>
        /// Chops a list into same-size smaller lists. For example:
        /// new int[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16 }.Chop(5)
        /// will return: { {1,2,3,4,5}, {6,7,8,9,10}, {11,12,13,14,15}, {16} }
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chop<T>(this IEnumerable<T> @this, int chopSize)
        {
            if (chopSize == 0 || @this.None())
            {
                yield return @this;
                yield break;
            }

            yield return @this.Take(chopSize);

            if (@this.Count() > chopSize)
            {
                var rest = @this.Skip(chopSize);

                foreach (var item in Chop(rest, chopSize))
                    yield return item;
            }
        }

        /// <summary>
        /// Returns the sum of a timespan selector on this list.
        /// </summary>
        public static TimeSpan Sum<T>(this IEnumerable<T> @this, Func<T, TimeSpan> selector)
        {
            var result = TimeSpan.Zero;
            foreach (var item in @this) result += selector(item);
            return result;
        }

        /// <summary>
        /// Returns the indices of all items which matche a specified criteria.
        /// </summary>
        public static IEnumerable<int> AllIndicesOf<T>(IEnumerable<T> list, Func<T, bool> criteria)
        {
            var index = 0;

            foreach (var item in list)
            {
                if (criteria(item)) yield return index;

                index++;
            }
        }

        /// <summary>
        /// Replaces the specified item in this list with the specified new item.
        /// </summary>
        public static void Replace<T>(this IList<T> @this, T oldItem, T newItem)
        {
            @this.Remove(oldItem);
            @this.Add(newItem);
        }

        /// <summary>
        /// Returns all elements of this list except those at the specified indices.
        /// </summary>
        public static IEnumerable<T> ExceptAt<T>(this IEnumerable<T> @this, params int[] indices) =>
            @this.Where((item, index) => !indices.Contains(index));

        /// <summary>
        /// Returns all elements of this list except the last X items.
        /// </summary>
        public static IEnumerable<T> ExceptLast<T>(this IEnumerable<T> @this, int count = 1)
        {
            var last = @this.Count();
            return @this.ExceptAt(Enumerable.Range(last - count, count).ToArray());
        }

        /// <summary>
        /// Returns all elements of this list except the first X items.
        /// </summary>
        public static IEnumerable<T> ExceptFirst<T>(this IEnumerable<T> @this, int count = 1) =>
            @this.ExceptAt(Enumerable.Range(0, count).ToArray());

        /// <summary>
        /// Removes the nulls from this list.
        /// </summary>
        public static void RemoveNulls<T>(this IList<T> @this) => @this.RemoveWhere(i => i == null);

        /// <summary>
        /// Determines whether this least contains at least the specified number of items.
        /// This can be faster than calling "x.Count() >= N" for complex iterators.
        /// </summary>
        public static bool ContainsAtLeast(this System.Collections.IEnumerable @this, int numberOfItems)
        {
            // Special case for List:
            if (@this is ICollection asList) return asList.Count >= numberOfItems;

            var itemsCount = 0;
            var result = itemsCount == numberOfItems;
            var enumerator = @this.GetEnumerator();

            while (enumerator.MoveNext())
            {
                itemsCount++;

                if (itemsCount >= numberOfItems) return true;
            }

            return result;
        }

        /// <summary>
        /// Adds the specified key/value pair to this list.
        /// </summary>
        public static KeyValuePair<TKey, TValue> Add<TKey, TValue>(this IList<KeyValuePair<TKey, TValue>> @this, TKey key, TValue value)
        {
            var result = new KeyValuePair<TKey, TValue>(key, value);
            @this.Add(result);

            return result;
        }

        /// <summary>
        /// Adds the specified items to this set.
        /// </summary>
        public static void AddRange<T>(this HashSet<T> @this, IEnumerable<T> items)
        {
            if (items == null) return;

            foreach (var item in items)
                @this.Add(item);
        }

        /// <summary>
        /// Dequeues all queued items in the right order.
        /// </summary>
        public static IEnumerable<T> DequeueAll<T>(this Queue<T> @this)
        {
            while (@this.Any())
                yield return @this.Dequeue();
        }

        /// <summary>
        /// Returns a HashSet of type T (use for performance in place of ToList()).
        /// </summary>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> @this) => new HashSet<T>(@this);

        /// <summary>
        /// Gets all indices of the specified item in this collection.
        /// </summary>
        public static IEnumerable<int> AllIndicesOf<T>(this IEnumerable<T> @this, T item)
        {
            var index = 0;
            foreach (var i in @this)
            {
                if (ReferenceEquals(item, null))
                {
                    if (ReferenceEquals(i, null)) yield return index;
                }
                else
                {
                    if (item.Equals(i)) yield return index;
                }

                index++;
            }
        }

        /// <summary>
        /// Returns an empty collection if this collection is null.
        /// </summary>
        [EscapeGCop("I am the solution to this GCop warning")]
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> @this) => @this ?? Enumerable.Empty<T>();

        /// <summary>
        /// Determines if the specified item exists in this list. 
        /// </summary>
        public static bool Contains<T>(this IEnumerable<T> @this, T? item) where T : struct
        {
            if (item == null) return false;

            return Enumerable.Contains(@this, item.Value);
        }

        /// <summary>
        /// Determines if the specified item exists in this list. 
        /// </summary>
        public static bool Lacks<T>(this IEnumerable<T> @this, T? item) where T : struct => !@this.Contains(item);

        /// <summary>
        /// Determines if this item is in the specified list.
        /// </summary>
        public static bool IsAnyOf<T>(this T? @this, IEnumerable<T> items) where T : struct
        {
            if (@this == null) return false;

            return items.Contains(@this.Value);
        }

        /// <summary>
        /// Determines if this item is in the specified list.
        /// </summary>
        public static bool IsAnyOf(this int @this, IEnumerable<int> items) => items.Contains(@this);

        /// <summary>
        /// Specifies whether this list contains any of the specified values.
        /// </summary>
        public static bool ContainsAny(this IEnumerable<Guid> @this, params Guid?[] ids) => @this.ContainsAny(ids.ExceptNull().ToArray());

        /// <summary>
        /// Finds the median of a list of integers
        /// </summary>
        public static int Median(this IEnumerable<int> @this)
        {
            @this = @this.ToList();

            if (@this.None())
                throw new ArgumentException("number list cannot be empty");

            var ordered = @this.OrderBy(i => i).ToList();

            var middle = @this.Count() / 2;

            if (@this.Count() % 2 == 1)
                return ordered.ElementAt(middle);

            // Return the average of the two middle numbers.
            return (ordered.ElementAt(middle - 1) + ordered.ElementAt(middle)) / 2;
        }

        /// <summary>
        /// If this list is null or empty, then the specified alternative will be returned, otherwise this will be returned.
        /// </summary>
        public static IEnumerable<T> Or<T>(this IEnumerable<T> @this, IEnumerable<T> valueIfEmpty)
        {
            if (@this.None()) return valueIfEmpty;
            else return @this;
        }

        public static Dictionary<T, K> ToDictionary<T, K>(this IEnumerable<KeyValuePair<T, K>> @this) =>
            @this.ToDictionary(x => x.Key, x => x.Value);

        public static IEnumerable<T> Except<T>(this IEnumerable<T> @this, Type excludedType)
        {
            if (@this == null) throw new NullReferenceException("No collection is given for the extension method Except().");

            if (excludedType == null) throw new ArgumentNullException(nameof(excludedType));

            var excludedTypeInfo = excludedType;

            if (excludedTypeInfo.IsClass)
            {
                foreach (var item in @this)
                {
                    if (item == null) yield return item;

                    var type = item.GetType();

                    if (type == excludedType) continue;
                    else if (type.IsSubclassOf(excludedType)) continue;
                    else yield return item;
                }
            }
            else if (excludedTypeInfo.IsInterface)
            {
                foreach (var item in @this)
                {
                    if (item == null) yield return item;

                    var type = item.GetType();

                    if (type.Implements(excludedType)) continue;
                    else yield return item;
                }
            }
            else throw new NotSupportedException("Except(System.Type) method does not recognize " + excludedType);

            // return list.Where(each => each == null || (each.GetType().IsAssignableFrom(excludedType) == false));
        }

        /// <summary>
        /// Creates a list of the specified runtime type including all items of this collection.
        /// </summary>
        public static IEnumerable Cast(this IEnumerable @this, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Generic List is in mscorelib, just like System.String:

            var listType = typeof(string).Assembly.GetType("System.Collections.Generic.List`1").MakeGenericType(type);
            var result = (IList)Activator.CreateInstance(listType);

            foreach (var item in @this)
                result.Add(item);

            return result;
        }

        public static IEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> @this, string propertyName)
        {
            if (propertyName.IsEmpty())
                throw new ArgumentNullException(nameof(propertyName));

            var property = typeof(TSource).GetProperty(propertyName);
            if (property == null)
                throw new ArgumentException("{0} is not a readable property of {1} type.".FormatWith(propertyName, typeof(TSource).FullName));

            return @this.OrderBy(new Func<TSource, object>((new PropertyComparer<TSource>(property)).ExtractValue<TSource, object>));
        }

        /// <summary>
        /// Sorts this list by the specified property name.
        /// </summary>
        public static IEnumerable OrderBy(this IEnumerable @this, string propertyName)
        {
            if (propertyName.IsEmpty())
                throw new ArgumentNullException(nameof(propertyName));

            if (propertyName.EndsWith(" DESC"))
                return OrderByDescending(@this, propertyName.TrimEnd(" DESC".Length));

            Type itemType = null;
            foreach (var item in @this)
            {
                itemType = item.GetType();
                break;
            }

            // Empty list:
            if (itemType == null) return @this;

            var property = itemType.GetProperty(propertyName);
            if (property == null) throw new ArgumentException("Unusable property name specified:" + propertyName);

            var comparer = new PropertyComparer(property);

            var result = new ArrayList();

            foreach (var item in @this)
                result.Add(item);

            result.Sort(comparer);

            return result;
        }

        public static IEnumerable OrderByDescending(this IEnumerable @this, string property)
        {
            if (property.IsEmpty()) throw new ArgumentNullException(nameof(property));

            var result = new ArrayList();
            foreach (var item in @this.OrderBy(property))
                result.Insert(0, item);

            return result;
        }

        public static IEnumerable<TSource> OrderByDescending<TSource>(this IEnumerable<TSource> @this, string propertyName)
        {
            if (propertyName.IsEmpty()) throw new ArgumentNullException(nameof(propertyName));

            var property = typeof(TSource).GetProperty(propertyName);

            if (property == null)
                throw new ArgumentException($"{propertyName} is not a readable property of {typeof(TSource).FullName}.");

            return @this.OrderByDescending(new Func<TSource, object>((new PropertyComparer(property)).ExtractValue<TSource, object>));
        }

        public static T FirstOrDefault<T>(this ICollection<T> @this, Func<T, bool> selector)
        {
            foreach (var item in @this)
                if (selector(item)) return item;

            return default(T);
        }

        public static Task AwaitAll<T>(this IEnumerable<T> @this, Func<T, Task> task)
        {
            var tasks = new List<Task>();

            if (@this != null)
                foreach (var item in @this)
                    tasks.Add(task?.Invoke(item) ?? Task.CompletedTask);

            return Task.WhenAll(tasks);
        }

        public static async Task<IEnumerable<T>> AwaitAll<T>(this IEnumerable<T> @this, Func<T, Task<T>> task)
        {
            var tasks = new List<Task<T>>();

            if (@this != null)
                foreach (var item in @this)
                    tasks.Add(task?.Invoke(item));

            await Task.WhenAll(tasks);

            return tasks.Select(x => x.GetAlreadyCompletedResult());
        }

        public static T[] PadRight<T>(this T[] @this, int size, T padItemValue)
        {
            if (@this.Length >= size) return @this;

            var result = new T[size];
            @this.CopyTo(result, 0);

            for (var i = @this.Length; i < size; i++)
                result[i] = padItemValue;

            return result;
        }

        /// <summary>
        /// If a specified condition is true, then the filter predicate will be executed.
        /// Otherwise the original list will be returned.
        /// </summary>
        [EscapeGCop("The condition param should not be last in this case.")]
        public static IEnumerable<T> FilterIf<T>(this IEnumerable<T> source,
             bool condition, Func<T, bool> predicate)
            => condition ? source.Where(predicate) : source;
    }
}