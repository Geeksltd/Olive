using System;
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

        public static string ToString(this IEnumerable list, string seperator)
        {
            if (list == null) return "{NULL}";
            return ToString(list.Cast<object>(), seperator);
        }

        public static async Task<string> ToString(this Task<IEnumerable> list, string seperator)
        {
            if (list == null) return "{NULL}";
            return (await list)?.ToString(seperator);
        }

        public static async Task<string> ToString<T>(this Task<IEnumerable<T>> list, string seperator)
        {
            if (list == null) return "{NULL}";
            return (await list)?.ToString(seperator);
        }

        public static string ToFormatString<T>(this IEnumerable<T> list, string format, string seperator, string lastSeperator) =>
            list.Select(i => format.FormatWith(i)).ToString(seperator, lastSeperator);

        public static bool Any<T>(this IEnumerable<T> list, Func<T, int, bool> predicate) => list.Any(predicate);

        public static bool None<T>(this IEnumerable<T> list, Func<T, int, bool> predicate) => !list.Any(predicate);

        public static string ToFormatString<T>(this IEnumerable<T> list, string format, string seperator)
            => list.Select(i => format.FormatWith(i)).ToString(seperator);

        public static string ToString<T>(this IEnumerable<T> list, string seperator)
            => ToString(list, seperator, seperator);

        public static T LastOrDefault<T>(this IEnumerable<T> @this) => @this.Reverse().FirstOrDefault();

        public static T LastOrDefault<T>(this IEnumerable<T> @this, Func<T, bool> pred)
            => @this.Reverse().FirstOrDefault(pred);

        public static string ToString<T>(this IEnumerable<T> list, string seperator, string lastSeperator)
        {
            var result = new StringBuilder();

            var items = list.Cast<object>().ToArray();

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

        public static int IndexOf<T>(this IEnumerable<T> list, T element)
        {
            if (list == null)
                throw new NullReferenceException("No collection is given for the extension method IndexOf().");

            if (list.Contains(element) == false) return -1;

            var result = 0;
            foreach (var el in list)
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
        public static int IndexOf<T>(this IEnumerable<T> list, Func<T, bool> criteria)
        {
            var result = 0;

            foreach (var item in list)
            {
                if (criteria(item)) return result;

                result++;
            }

            return -1;
        }

        public static void RemoveWhere<T>(this IList<T> list, Func<T, bool> selector)
        {
            lock (list)
            {
                var itemsToRemove = list.Where(selector).ToList();
                list.Remove(itemsToRemove);
            }
        }

        /// <summary>
        /// Gets all items of this list except those meeting a specified criteria.
        /// </summary>
        /// <param name="criteria">Exclusion criteria</param>
        [EscapeGCop("It is the Except definition and so it cannot call itself")]
        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, Func<T, bool> criteria) => list.Where(i => !criteria(i));

        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, T item) => list.Except(new T[] { item });

        [EscapeGCop("It is the Except definition and so it cannot call itself")]
        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, params T[] items)
        {
            if (items == null) return list;

            return list.Where(x => !items.Contains(x));
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, List<T> itemsToExclude) => list.Except(itemsToExclude.ToArray());

        public static IEnumerable<char> Except(this IEnumerable<char> list, IEnumerable<char> itemsToExclude) =>
            list.Except(itemsToExclude.ToArray());

        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, IEnumerable<T> itemsToExclude, bool alsoDistinct = false)
        {
            var result = list.Except(itemsToExclude.ToArray());

            if (alsoDistinct) result = result.Distinct();

            return result;
        }

        public static IEnumerable<string> Except(this IEnumerable<string> list, IEnumerable<string> itemsToExclude) =>
            list.Except(itemsToExclude.ToArray());

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<T> ExceptNull<T>(this IEnumerable<T> list) where T : class => list.Where(i => i != null);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<int> ExceptNull(this IEnumerable<int?> list) =>
            list.Where(i => i.HasValue).Select(x => x.Value);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<double> ExceptNull(this IEnumerable<double?> list) =>
            list.Where(i => i.HasValue).Select(x => x.Value);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<TimeSpan> ExceptNull(this IEnumerable<TimeSpan?> list) =>
            list.Where(i => i.HasValue).Select(x => x.Value);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<decimal> ExceptNull(this IEnumerable<decimal?> list) =>
            list.Where(i => i.HasValue).Select(x => x.Value);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<bool> ExceptNull(this IEnumerable<bool?> list) => list.Where(i => i.HasValue).Select(x => x.Value);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<DateTime> ExceptNull(this IEnumerable<DateTime?> list) =>
            list.Where(i => i.HasValue).Select(x => x.Value);

        /// <summary>
        /// Gets all Non-NULL items of this list.
        /// </summary>
        public static IEnumerable<Guid> ExceptNull(this IEnumerable<Guid?> list) => list.Where(i => i.HasValue).Select(x => x.Value);

        public static bool IsSingle<T>(this IEnumerable<T> list) => IsSingle<T>(list, x => true);

        public static bool IsSingle<T>(this IEnumerable<T> list, Func<T, bool> criteria)
        {
            var visitedAny = false;

            foreach (var item in list.Where(criteria))
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
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> list, IEnumerable<T> prefix) => prefix.Concat(list);

        /// <summary>
        /// Adds the specified item(s) to the beginning of this list.
        /// </summary>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> list, params T[] prefix) => prefix.Concat(list);

        /// <summary>
        /// Performs an action for all items within the list.
        /// </summary>
        public static void Do<T>(this IEnumerable<T> list, ItemHandler<T> action)
        {
            if (list == null) return;

            foreach (var item in list)
                action?.Invoke(item);
        }

        /// <summary>
        /// Performs an action for all items within the list.
        /// </summary>
        public static async Task Do<T>(this IEnumerable<T> list, Func<T, Task> func)
        {
            if (list == null || func == null) return;

            foreach (var item in list) await func(item);
        }

        /// <summary>
        /// Performs an action for all items within the list.
        /// It will provide the index of the item in the list to the action handler as well.
        /// </summary>        
        public static void Do<T>(this IEnumerable<T> list, Action<T, int> action)
        {
            if (list == null || action == null) return;

            var index = 0;

            foreach (var item in list)
            {
                action(item, index);
                index++;
            }
        }

        /// <summary>
        /// Performs an action for all items within the list.
        /// It will provide the index of the item in the list to the action handler as well.
        /// </summary>        
        public static async Task DoAsync<T>(this IEnumerable<T> list, Func<T, int, Task> action)
        {
            if (list == null || action == null) return;

            var index = 0;

            foreach (var item in list)
            {
                await action(item, index);
                index++;
            }
        }

        public delegate void ItemHandler<in T>(T arg);

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> list, T item) => list.Concat(new T[] { item });

        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
                list.Add(item);
        }

        /// <summary>
        /// Gets the minimum value of a specified expression in this list. If the list is empty, then the default value of the expression will be returned.
        /// </summary>
        public static R MinOrDefault<T, R>(this IEnumerable<T> list, Func<T, R> expression)
        {
            if (list.None()) return default(R);
            return list.Min(expression);
        }

        /// <summary>
        /// Gets the maximum value of a specified expression in this list. If the list is empty, then the default value of the expression will be returned.
        /// </summary>
        public static R MaxOrDefault<T, R>(this IEnumerable<T> list, Func<T, R> expression)
        {
            if (list.None()) return default(R);
            return list.Max(expression);
        }

        /// <summary>
        /// Gets the maximum value of the specified expression in this list. 
        /// If no items exist in the list then null will be returned. 
        /// </summary>     
        public static R? MaxOrNull<T, R>(this IEnumerable<T> list, Func<T, R?> expression) where R : struct
        {
            if (list.None()) return default(R?);
            return list.Max(expression);
        }

        /// <summary>
        /// Gets the maximum value of the specified expression in this list. 
        /// If no items exist in the list then null will be returned. 
        /// </summary>     
        public static R? MaxOrNull<T, R>(this IEnumerable<T> list, Func<T, R> expression) where R : struct =>
            list.MaxOrNull(item => (R?)expression(item));

        /// <summary>
        /// Gets the minimum value of the specified expression in this list. 
        /// If no items exist in the list then null will be returned. 
        /// </summary>     
        public static R? MinOrNull<T, R>(this IEnumerable<T> list, Func<T, R?> expression) where R : struct
        {
            if (list.None()) return default(R?);
            return list.Min(expression);
        }

        /// <summary>
        /// Gets the minimum value of the specified expression in this list. 
        /// If no items exist in the list then null will be returned. 
        /// </summary>     
        public static R? MinOrNull<T, R>(this IEnumerable<T> list, Func<T, R> expression) where R : struct =>
            list.MinOrNull(item => (R?)expression(item));

        public static bool IsSubsetOf<T>(this IEnumerable<T> source, IEnumerable<T> target) => target.ContainsAll(source);

        /// <summary>
        /// Determines whether this list is equivalent to another specified list. Items in the list should be distinct for accurate result.
        /// </summary>
        public static bool IsEquivalentTo<T>(this IEnumerable<T> list, IEnumerable<T> other)
        {
            if (list == null) list = new T[0];
            if (other == null) other = new T[0];

            if (list.Count() != other.Count()) return false;

            foreach (var item in list)
                if (!other.Contains(item)) return false;
            return true;
        }

        /// <summary>
        /// Counts the number of items in this list matching the specified criteria.
        /// </summary>
        public static int Count<T>(this IEnumerable<T> list, Func<T, int, bool> criteria) => list.Count((x, i) => criteria(x, i));

        /// <summary>
        /// Picks an item from the list.
        /// </summary>
        public static T PickRandom<T>(this IEnumerable<T> list)
        {
            if (list.Any())
            {
                var index = RandomProvider.Next(list.Count());
                return list.ElementAt(index);
            }
            else return default(T);
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> list, int number)
        {
            if (number < 1) throw new ArgumentException("number should be greater than 0.");

            var items = list as List<T> ?? list.ToList();

            if (number >= items.Count) number = items.Count;

            var randomIndices = RandomProvider.PickNumbers(number, 0, items.Count - 1);

            foreach (var index in randomIndices)
                yield return items[index];
        }

        /// <summary>
        /// Works as opposite of Contains().
        /// </summary>        
        public static bool Lacks<T>(this IEnumerable<T> list, T item) => !list.Contains(item);

        /// <summary>
        /// Determines if this list lacks any item in the specified list.
        /// </summary>        
        public static bool LacksAny<T>(this IEnumerable<T> list, IEnumerable<T> items) => !list.ContainsAll(items);

        /// <summary>
        /// Determines if this list lacks all items in the specified list.
        /// </summary>        
        public static bool LacksAll<T>(this IEnumerable<T> list, IEnumerable<T> items) => !list.ContainsAny(items.ToArray());

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> list)
        {
            if (list.None()) return new T[0];

            var items = list.ToList();

            return PickRandom(items, items.Count);
        }

        /// <summary>
        /// Returns a subset of the items in this collection.
        /// </summary>
        public static IEnumerable<T> Take<T>(this IEnumerable<T> list, int lowerBound, int count)
        {
            if (lowerBound < 0) throw new ArgumentOutOfRangeException(nameof(lowerBound));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return Enumerable.Empty<T>();

            return list.Skip(lowerBound).Take(count);
        }

        public static IEnumerable<T> Distinct<T, TResult>(this IEnumerable<T> list, Func<T, TResult> selector)
        {
            var keys = new List<TResult>();

            foreach (var item in list)
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
        public static bool ContainsAll<T>(this IEnumerable<T> list, IEnumerable<T> items) =>
            items.All(i => list.Contains(i));

        /// <summary>
        /// Determines if this list contains any of the specified items.
        /// </summary>
        public static bool ContainsAny<T>(this IEnumerable<T> list, params T[] items) => list.Intersects(items);

        /// <summary>
        /// Determines if none of the items in this list meet a given criteria.
        /// </summary>
        public static bool None<T>(this IEnumerable<T> list, Func<T, bool> criteria) => !list.Any(criteria);

        /// <summary>
        /// A null safe alternative to Any(). If the source is null it will return false instead of throwing an exception.
        /// </summary>
        public static bool HasAny<TSource>(this IEnumerable<TSource> source)
           => source != null && source.Any();

        /// <summary>
        /// Determines if this is null or an empty list.
        /// </summary>
        public static bool None<T>(this IEnumerable<T> list) => !list.HasAny();

        /// <summary>
        /// Determines if this list intersects with another specified list.
        /// </summary>
        public static bool Intersects<T>(this IEnumerable<T> list, IEnumerable<T> otherList)
        {
            var countList = (list as ICollection)?.Count;
            var countOther = (otherList as ICollection)?.Count;

            if (countList == null || countOther == null || countOther < countList)
            {
                foreach (var item in otherList)
                    if (list.Contains(item)) return true;
            }
            else
            {
                foreach (var item in list)
                    if (otherList.Contains(item)) return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if this list intersects with another specified list.
        /// </summary>
        public static bool Intersects<T>(this IEnumerable<T> list, params T[] items) => list.Intersects((IEnumerable<T>)items);

        /// <summary>
        /// Selects the item with maximum of the specified value.
        /// If this list is empty, NULL (or default of T) will be returned.
        /// </summary>
        public static T WithMax<T, TKey>(this IEnumerable<T> list, Func<T, TKey> keySelector)
        {
            if (list.None()) return default(T);
            return list.Aggregate((a, b) => Comparer.Default.Compare(keySelector(a), keySelector(b)) > 0 ? a : b);
        }

        /// <summary>
        /// Selects the item with minimum of the specified value.
        /// </summary>
        public static T WithMin<T, TKey>(this IEnumerable<T> list, Func<T, TKey> keySelector)
        {
            if (list.None()) return default(T);
            return list.Aggregate((a, b) => Comparer.Default.Compare(keySelector(a), keySelector(b)) < 0 ? a : b);
        }

        /// <summary>
        /// Gets the element after a specified item in this list.
        /// If the specified element does not exist in this list, an ArgumentException will be thrown.
        /// If the specified element is the last in the list, NULL will be returned.
        /// </summary>        
        public static T GetElementAfter<T>(this IEnumerable<T> list, T item) where T : class
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var index = list.IndexOf(item);
            if (index == -1)
                throw new ArgumentException("The specified item does not exist to this list.");

            if (index == list.Count() - 1) return null;

            return list.ElementAt(index + 1);
        }

        /// <summary>
        /// Gets the element before a specified item in this list.
        /// If the specified element does not exist in this list, an ArgumentException will be thrown.
        /// If the specified element is the first in the list, NULL will be returned.
        /// </summary>        
        public static T GetElementBefore<T>(this IEnumerable<T> list, T item) where T : class
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var index = list.IndexOf(item);
            if (index == -1)
                throw new ArgumentException("The specified item does not exist to this list.");

            if (index == 0) return null;

            return list.ElementAt(index - 1);
        }

        public static void AddFormat(this IList<string> list, string format, params object[] arguments) =>
            list.Add(string.Format(format, arguments));

        public static void AddFormattedLine(this IList<string> list, string format, params object[] arguments) =>
            list.Add(string.Format(format + Environment.NewLine, arguments));

        public static void AddLine(this IList<string> list, string text) => list.Add(text + Environment.NewLine);

        /// <summary>
        /// Removes a list of items from this list.
        /// </summary>
        public static void Remove<T>(this IList<T> list, IEnumerable<T> itemsToRemove)
        {
            if (itemsToRemove != null)
            {
                foreach (var item in itemsToRemove)
                    if (list.Contains(item)) list.Remove(item);
            }
        }

        /// <summary>
        /// Determines if all items in this collection are unique.
        /// </summary>
        public static bool AreItemsUnique<T>(this IEnumerable<T> collection)
        {
            if (collection.None()) return true;

            return collection.Distinct().Count() == collection.Count();
        }

        /// <summary>
        /// Returns the union of this list with the specified other lists.
        /// </summary>

        public static IEnumerable<T> Union<T>(this IEnumerable<T> list, params IEnumerable<T>[] otherLists)
        {
            var result = list;

            foreach (var other in otherLists)
                result = Enumerable.Union(result, other);

            return result;
        }

        /// <summary>
        /// Returns the union of this list with the specified items.
        /// </summary>
        public static IEnumerable<T> Union<T>(this IEnumerable<T> list, params T[] otherItems) => list.Union((IEnumerable<T>)otherItems);

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> list, params IEnumerable<T>[] otherLists)
        {
            var result = list;

            foreach (var other in otherLists) result = Enumerable.Concat(result, other);

            return result;
        }

        /// <summary>
        /// Gets the average of the specified expression on all items of this list.
        /// If the list is empty, null will be returned.
        /// </summary>
        public static double? AverageOrDefault<T>(this IEnumerable<T> list, Func<T, int> selector)
        {
            if (list.None()) return null;
            else return list.Average(selector);
        }

        /// <summary>
        /// Gets the average of the specified expression on all items of this list.
        /// If the list is empty, null will be returned.
        /// </summary>
        public static double? AverageOrDefault<T>(this IEnumerable<T> list, Func<T, int?> selector)
        {
            if (list.None()) return null;
            else return list.Average(selector);
        }

        /// <summary>
        /// Gets the average of the specified expression on all items of this list.
        /// If the list is empty, null will be returned.
        /// </summary>
        public static double? AverageOrDefault<T>(this IEnumerable<T> list, Func<T, double> selector)
        {
            if (list.None()) return null;
            else return list.Average(selector);
        }

        /// <summary>
        /// Gets the average of the specified expression on all items of this list.
        /// If the list is empty, null will be returned.
        /// </summary>
        public static double? AverageOrDefault<T>(this IEnumerable<T> list, Func<T, double?> selector)
        {
            if (list.None()) return null;
            else return list.Average(selector);
        }

        /// <summary>
        /// Gets the average of the specified expression on all items of this list.
        /// If the list is empty, null will be returned.
        /// </summary>
        public static decimal? AverageOrDefault<T>(this IEnumerable<T> list, Func<T, decimal> selector)
        {
            if (list.None()) return null;
            else return list.Average(selector);
        }

        /// <summary>
        /// Gets the average of the specified expression on all items of this list.
        /// If the list is empty, null will be returned.
        /// </summary>
        public static decimal? AverageOrDefault<T>(this IEnumerable<T> list, Func<T, decimal?> selector)
        {
            if (list.None()) return null;
            else return list.Average(selector);
        }

        /// <summary>
        /// Trims all elements in this list and excludes all null and "empty string" elements from the list.
        /// </summary>
        public static IEnumerable<string> Trim(this IEnumerable<string> list)
        {
            if (list == null) return Enumerable.Empty<string>();

            return list.Except(v => string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Where(v => v.HasValue()).ToArray();
        }

        /// <summary>
        /// Determines whether this list of strings contains the specified string.
        /// </summary>
        public static bool Contains(this IEnumerable<string> list, string instance, bool caseSensitive)
        {
            if (caseSensitive || instance.IsEmpty())
                return list.Contains(instance);
            else return list.Any(i => i.HasValue() && i.Equals(instance, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines whether this list of strings contains the specified string.
        /// </summary>
        public static bool Lacks(this IEnumerable<string> list, string instance, bool caseSensitive) =>
            !Contains(list, instance, caseSensitive);

        /// <summary>
        /// Concats all elements in this list with Environment.NewLine.
        /// </summary>
        public static string ToLinesString<T>(this IEnumerable<T> list) => list.ToString(Environment.NewLine);

        /// <summary>
        /// Chops a list into same-size smaller lists. For example:
        /// new int[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16 }.Chop(5)
        /// will return: { {1,2,3,4,5}, {6,7,8,9,10}, {11,12,13,14,15}, {16} }
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chop<T>(this IEnumerable<T> list, int chopSize)
        {
            if (chopSize == 0 || list.None())
            {
                yield return list;
                yield break;
            }

            yield return list.Take(chopSize);

            if (list.Count() > chopSize)
            {
                var rest = list.Skip(chopSize);

                foreach (var item in Chop(rest, chopSize))
                    yield return item;
            }
        }

        /// <summary>
        /// Returns the sum of a timespan selector on this list.
        /// </summary>
        public static TimeSpan Sum<T>(this IEnumerable<T> list, Func<T, TimeSpan> selector)
        {
            var result = TimeSpan.Zero;
            foreach (var item in list) result += selector(item);
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
        public static void Replace<T>(this IList<T> list, T oldItem, T newItem)
        {
            list.Remove(oldItem);
            list.Add(newItem);
        }

        /// <summary>
        /// Returns all elements of this list except those at the specified indices.
        /// </summary>
        public static IEnumerable<T> ExceptAt<T>(this IEnumerable<T> list, params int[] indices) =>
            list.Where((item, index) => !indices.Contains(index));

        /// <summary>
        /// Returns all elements of this list except the last X items.
        /// </summary>
        public static IEnumerable<T> ExceptLast<T>(this IEnumerable<T> list, int count = 1)
        {
            var last = list.Count();
            return list.ExceptAt(Enumerable.Range(last - count, count).ToArray());
        }

        /// <summary>
        /// Returns all elements of this list except the first X items.
        /// </summary>
        public static IEnumerable<T> ExceptFirst<T>(this IEnumerable<T> list, int count = 1) =>
            list.ExceptAt(Enumerable.Range(0, count).ToArray());

        /// <summary>
        /// Removes the nulls from this list.
        /// </summary>
        public static void RemoveNulls<T>(this IList<T> list) => list.RemoveWhere(i => i == null);

        /// <summary>
        /// Determines whether this least contains at least the specified number of items.
        /// This can be faster than calling "x.Count() >= N" for complex iterators.
        /// </summary>
        public static bool ContainsAtLeast(this System.Collections.IEnumerable list, int numberOfItems)
        {
            // Special case for List:
            if (list is ICollection asList) return asList.Count >= numberOfItems;

            var itemsCount = 0;
            var result = itemsCount == numberOfItems;
            var enumerator = list.GetEnumerator();

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
        public static KeyValuePair<TKey, TValue> Add<TKey, TValue>(this IList<KeyValuePair<TKey, TValue>> list, TKey key, TValue value)
        {
            var result = new KeyValuePair<TKey, TValue>(key, value);
            list.Add(result);

            return result;
        }

        /// <summary>
        /// Adds the specified items to this set.
        /// </summary>
        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
        {
            if (items == null) return;

            foreach (var item in items)
                set.Add(item);
        }

        /// <summary>
        /// Dequeues all queued items in the right order.
        /// </summary>
        public static IEnumerable<T> DequeueAll<T>(this Queue<T> @this)
        {
            while (@this.Count > 0)
                yield return @this.Dequeue();
        }

        /// <summary>
        /// Returns a HashSet of type T (use for performance in place of ToList()).
        /// </summary>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection) => new HashSet<T>(collection);

        /// <summary>
        /// Gets all indices of the specified item in this collection.
        /// </summary>
        public static IEnumerable<int> AllIndicesOf<T>(this IEnumerable<T> all, T item)
        {
            var index = 0;
            foreach (var i in all)
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
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> collection) => collection ?? Enumerable.Empty<T>();

        /// <summary>
        /// Determines if the specified item exists in this list. 
        /// </summary>
        public static bool Contains<T>(this IEnumerable<T> items, T? item) where T : struct
        {
            if (item == null) return false;

            return Enumerable.Contains(items, item.Value);
        }

        /// <summary>
        /// Determines if the specified item exists in this list. 
        /// </summary>
        public static bool Lacks<T>(this IEnumerable<T> items, T? item) where T : struct => !items.Contains(item);

        /// <summary>
        /// Determines if this item is in the specified list.
        /// </summary>
        public static bool IsAnyOf<T>(this T? item, IEnumerable<T> items) where T : struct
        {
            if (item == null) return false;

            return items.Contains(item.Value);
        }

        /// <summary>
        /// Determines if this item is in the specified list.
        /// </summary>
        public static bool IsAnyOf(this int item, IEnumerable<int> items) => items.Contains(item);

        /// <summary>
        /// Specifies whether this list contains any of the specified values.
        /// </summary>
        public static bool ContainsAny(this IEnumerable<Guid> list, params Guid?[] ids) => list.ContainsAny(ids.ExceptNull().ToArray());

        /// <summary>
        /// Finds the median of a list of integers
        /// </summary>
        public static int Median(this IEnumerable<int> numbers)
        {
            numbers = numbers.ToList();

            if (numbers.None())
                throw new ArgumentException("number list cannot be empty");

            var ordered = numbers.OrderBy(i => i).ToList();

            var middle = numbers.Count() / 2;

            if (numbers.Count() % 2 == 1)
                return ordered.ElementAt(middle);

            // Return the average of the two middle numbers.
            return (ordered.ElementAt(middle - 1) + ordered.ElementAt(middle)) / 2;
        }

        /// <summary>
        /// If this list is null or empty, then the specified alternative will be returned, otherwise this will be returned.
        /// </summary>
        public static IEnumerable<T> Or<T>(this IEnumerable<T> list, IEnumerable<T> valueIfEmpty)
        {
            if (list.None()) return valueIfEmpty;
            else return list;
        }

        public static Dictionary<T, K> ToDictionary<T, K>(this IEnumerable<KeyValuePair<T, K>> items) =>
            items.ToDictionary(x => x.Key, x => x.Value);

        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, Type excludedType)
        {
            if (list == null) throw new NullReferenceException("No collection is given for the extension method Except().");

            if (excludedType == null) throw new ArgumentNullException(nameof(excludedType));

            var excludedTypeInfo = excludedType;

            if (excludedTypeInfo.IsClass)
            {
                foreach (var item in list)
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
                foreach (var item in list)
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
        public static IEnumerable Cast(this IEnumerable list, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Generic List is in mscorelib, just like System.String:

            var listType = typeof(string).Assembly.GetType("System.Collections.Generic.List`1").MakeGenericType(type);
            var result = (IList)Activator.CreateInstance(listType);

            foreach (var item in list)
                result.Add(item);

            return result;
        }

        public static IEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> list, string propertyName)
        {
            if (propertyName.IsEmpty())
                throw new ArgumentNullException(nameof(propertyName));

            var property = typeof(TSource).GetProperty(propertyName);
            if (property == null)
                throw new ArgumentException("{0} is not a readable property of {1} type.".FormatWith(propertyName, typeof(TSource).FullName));

            return list.OrderBy(new Func<TSource, object>((new PropertyComparer<TSource>(property)).ExtractValue<TSource, object>));
        }

        /// <summary>
        /// Sorts this list by the specified property name.
        /// </summary>
        public static IEnumerable OrderBy(this IEnumerable list, string propertyName)
        {
            if (propertyName.IsEmpty())
                throw new ArgumentNullException(nameof(propertyName));

            if (propertyName.EndsWith(" DESC"))
                return OrderByDescending(list, propertyName.TrimEnd(" DESC".Length));

            Type itemType = null;
            foreach (var item in list)
            {
                itemType = item.GetType();
                break;
            }

            // Empty list:
            if (itemType == null) return list;

            var property = itemType.GetProperty(propertyName);
            if (property == null) throw new ArgumentException("Unusable property name specified:" + propertyName);

            var comparer = new PropertyComparer(property);

            var result = new ArrayList();

            foreach (var item in list)
                result.Add(item);

            result.Sort(comparer);

            return result;
        }

        public static IEnumerable OrderByDescending(this IEnumerable list, string property)
        {
            if (string.IsNullOrEmpty(property)) throw new ArgumentNullException(nameof(property));

            var result = new ArrayList();
            foreach (var item in list.OrderBy(property))
                result.Insert(0, item);

            return result;
        }

        public static IEnumerable<TSource> OrderByDescending<TSource>(this IEnumerable<TSource> list, string propertyName)
        {
            if (propertyName.IsEmpty()) throw new ArgumentNullException(nameof(propertyName));

            var property = typeof(TSource).GetProperty(propertyName);

            if (property == null)
                throw new ArgumentException($"{propertyName} is not a readable property of {typeof(TSource).FullName}.");

            return list.OrderByDescending(new Func<TSource, object>((new PropertyComparer(property)).ExtractValue<TSource, object>));
        }

        public static T FirstOrDefault<T>(this ICollection<T> collection, Func<T, bool> selector)
        {
            foreach (var item in collection)
                if (selector(item)) return item;

            return default(T);
        }

        public static Task AwaitAll<T>(this IEnumerable<T> list, Func<T, Task> task)
        {
            var tasks = new List<Task>();

            if (list != null)
                foreach (var item in list)
                    tasks.Add(task?.Invoke(item) ?? Task.CompletedTask);

            return Task.WhenAll(tasks);
        }

        public static async Task<IEnumerable<T>> AwaitAll<T>(this IEnumerable<T> list, Func<T, Task<T>> task)
        {
            var tasks = new List<Task<T>>();

            if (list != null)
                foreach (var item in list)
                    tasks.Add(task?.Invoke(item));

            await Task.WhenAll(tasks);

            return tasks.Select(x => x.GetAlreadyCompletedResult());
        }

        public static T[] PadRight<T>(this T[] array, int size, T padItemValue)
        {
            if (array.Length >= size) return array;

            var result = new T[size];
            array.CopyTo(result, 0);

            for (var i = array.Length; i < size; i++)
                result[i] = padItemValue;

            return result;
        }
    }
}