using System;
using System.Collections.Generic;
using System.Linq;

namespace Olive
{
    partial class OliveExtensions
    {
        /// <summary>
        /// Returns a formatted string based on this Range&lt;DateTime&gt; object and the given string format.
        /// </summary>
        public static string ToString(this Range<DateTime> @this, string format)
        {
            if (@this.From == DateTime.MinValue && @this.To == DateTime.MinValue) return string.Empty;

            if ("A".Equals(format, StringComparison.OrdinalIgnoreCase))
            {
                if (@this.To != DateTime.MaxValue) return "{0:d}-{1:d}".FormatWith(@this.From, @this.To);
                else return "{0:d}-...".FormatWith(@this.From);
            }

            if ("F".Equals(format, StringComparison.OrdinalIgnoreCase))
            {
                if (@this.To != DateTime.MaxValue) return "From {0:d} to {1:d}".FormatWith(@this.From, @this.To);
                else return "From {0:d}".FormatWith(@this.From);
            }

            if ("T".Equals(format, StringComparison.OrdinalIgnoreCase))
            {
                if (@this.To != DateTime.MaxValue) return "{0:d} to {1:d}".FormatWith(@this.From, @this.To);
                else return "{0:d}".FormatWith(@this.From);
            }

            return @this.ToString();
        }

        /// <summary>
        /// Gets all possible items in the range based on the specified intervals.
        /// </summary>
        public static IEnumerable<double> GetIntervals(this Range<double> range, double interval)
        {
            if (interval <= 0) throw new Exception("Interval should be a positive number.");

            for (var item = range.From; item <= range.To; item += interval)
                yield return item;
        }

        /// <summary>
        /// Gets all possible items in the range based on the specified intervals.
        /// </summary>
        public static IEnumerable<decimal> GetIntervals(this Range<decimal> range, decimal interval)
        {
            if (interval <= 0) throw new Exception("Interval should be a positive number.");

            for (var item = range.From; item <= range.To; item += interval)
                yield return item;
        }

        /// <summary>
        /// Merges adjecant items in this list if their gap is within the specified tolerance.
        /// The result will be another list of ranges with potentially fewer (but larger) ranges.
        /// Consider sorting the items before calling this method.
        /// </summary>
        public static IEnumerable<Range<DateTime>> MergeAdjacents(this IEnumerable<Range<DateTime>> items, TimeSpan tolerance)
        {
            Range<DateTime> last = null;

            foreach (var item in items)
            {
                if (last == null)
                {
                    last = item;
                    continue;
                }

                var difference = item.From.Subtract(last.To);
                if (difference < TimeSpan.Zero) difference = -difference;

                if (difference > tolerance)
                {
                    yield return last;
                    last = item;
                }
                else
                {
                    last = new Range<DateTime>(last.From, item.To);
                }
            }

            if (last != null) yield return last;
        }

        /// <summary>
        /// Determines if there is any overlap between any two ranges in this list of ranges.
        /// </summary>
        public static bool Overlap<T>(this IEnumerable<Range<T>> ranges, bool includeEdges = true)
            where T : IComparable, IComparable<T>
        {
            var all = ranges.ExceptNull().ToArray();

            for (var i = 0; i < all.Length; i++)
                for (var j = i + 1; j < all.Length; j++)
                    if (all[i].Intersects(all[j], includeEdges)) return true;

            return false;
        }
    }
}