namespace Olive
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides a collection of ranges to simplify the manipulation of them.
    /// This class is not thread-safe.
    /// </summary>
    [Serializable]
    public class IntRangeCollection : IEnumerable<int>
    {
        SortedList<int, Range<int>> ranges = new SortedList<int, Range<int>>();

        public IntRangeCollection() { }

        public IntRangeCollection(IEnumerable<Range<int>> ranges)
        {
            if (ranges is null) return;

            var array = ranges.ToArray();

            for (var index = 0; index < array.Length; index++)
                for (var inner = index + 1; inner < array.Length; inner++)
                    if (array[index].Intersects(array[inner]))
                        throw new ArgumentException("The initializing ranges should not intersect each other.");

            ranges.Do(r => this.ranges.Add(r.From, r));
        }

        public override string ToString()
        {
            return ranges.Select(x => x.Value.From.Equals(x.Value.To) ? x.Value.From.ToString() : x.Value.ToString("-")).ToString("|");
        }

        static Range<int> ExtractRange(string text)
        {
            try
            {
                var parts = text.Split('-');
                var from = parts[0].To<int>();
                if (parts.Length == 1) return new Range<int>(from, from);
                return new Range<int>(from, parts[1].To<int>());
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to extract Range<int> from " + text, ex);
            }
        }

        static List<Range<int>> ExtractRanges(string text)
        {
            var items = text.OrEmpty().Split('|').Trim().ToArray();
            return items.Select(ExtractRange).ToList();
        }

        public static IntRangeCollection Parse(string text)
        {
            var ranges = ExtractRanges(text);

            for (var index = ranges.Count - 1; index > 0; index--)
            {
                var range = (Range<int>)(object)ranges[index];
                var prevRange = (Range<int>)(object)ranges[index - 1];

                if (prevRange.To == range.From - 1)
                {
                    ranges.RemoveAt(index);
                    index = Math.Min(index + 1, ranges.Count - 1);
                    prevRange.To = range.To;
                }
            }

            var result = new IntRangeCollection();
            foreach (var item in ranges) result.ranges.Add(item.From, item);
            return result;
        }

        public Range<int>[] Ranges => ranges.Values.ToArray();

        public void Add(int item)
        {
            var search = DetectRange(item);
            if (search.ExistingRangeStartingWith.HasValue) return;

            if (search.WhereItBelongs == 0) AppendToStart(item);
            else AppendAt(item, search.WhereItBelongs);
        }

        void AppendToStart(int item)
        {
            if (ranges.Any())
            {
                var firstRange = ranges.Values.First();
                var nextItem = item + 1;

                var canJustExtendTheRange = firstRange.From.Equals(nextItem);

                if (canJustExtendTheRange)
                {
                    ExtendFromLowerBound(item, firstRange);
                    return;
                }
            }

            AddAsNewRange(item);
        }

        Range<int> GetRange(int index) => ranges.Values.ElementAtOrDefault(index);

        void AppendAt(int item, int index)
        {
            var range = GetRange(index);
            if (range?.Contains(item) == true) return;

            if (range?.To == item - 1)
            {
                ExtendFromUpperBound(item, range);
                return;
            }

            if (index + 1 < ranges.Count)
            {
                range = ranges.Values[index + 1];

                if (range.From == item + 1)
                {
                    ExtendFromLowerBound(item, range);
                    return;
                }
            }

            AddAsNewRange(item);
        }

        public void Clear() => ranges.Clear();

        public bool Contains(int item)
        {
            var index = ranges.Keys.ToList().BinarySearch(item);
            if (index >= 0) return true;
            index = -(index + 1);
            if (index >= ranges.Count) return false;

            if (ranges.Values[index].Contains(item)) return true;
            return index > 1 && (ranges.Values[index - 1].Contains(item));
        }

        public bool Lacks(int item) => !Contains(item);

        public bool Remove(int item)
        {
            var search = DetectRange(item);

            // The item equals one of ranges` from value;
            if (search.ExistingRangeStartingWith.HasValue)
            {
                ShrinkFromLowerBound(item, search.ExistingRangeStartingWith.Value);
                return true;
            }

            if (search.WhereItBelongs == 0) return false;

            var range = GetRange(search.WhereItBelongs - 1);

            if (range.To == item)
                ShrinkFromUpperBound(range);
            else if (range.Contains(item))
                Split(item, range);

            return true;
        }

        void Split(int deadItem, Range<int> range)
        {
            var newRange1 = new Range<int>(range.From, deadItem - 1);
            var newRange2 = new Range<int>(deadItem + 1, range.To);

            ranges.Remove(range.From);
            ranges.Add(newRange1.From, newRange1);
            ranges.Add(newRange2.From, newRange2);
        }

        void ShrinkFromUpperBound(Range<int> range)
        {
            var newRange = new Range<int>(range.From, range.To - 1);
            ranges.Remove(range.From);
            ranges.Add(newRange.From, newRange);
        }

        void ShrinkFromLowerBound(int item, int index)
        {
            var range = ranges.Values[index];
            ranges.Remove(item);

            if (range.From.Equals(range.To)) return;

            var newRange = new Range<int>(item + 1, range.To);
            ranges.Add(newRange.From, newRange);
        }

        void AddAsNewRange(int item) => ranges.Add(item, new Range<int>(item, item));

        void ExtendFromUpperBound(int item, Range<int> range)
        {
            var newRange = new Range<int>(range.From, item);

            ranges.Remove(range.From);
            ranges.Add(newRange.From, newRange);
        }

        void ExtendFromLowerBound(int item, Range<int> range)
        {
            var newRange = new Range<int>(item, range.To);

            ranges.Remove(range.From);
            ranges.Add(newRange.From, newRange);
        }

        class RangeIndexSearchResult
        {
            public int? ExistingRangeStartingWith;
            public int WhereItBelongs;
        }

        RangeIndexSearchResult DetectRange(int item)
        {
            var index = ranges.Keys.ToList().BinarySearch(item);
            if (index >= 0) return new RangeIndexSearchResult { ExistingRangeStartingWith = index };
            else return new RangeIndexSearchResult { WhereItBelongs = -(index + 1) };
        }

        public IEnumerator<int> GetEnumerator()
        {
            foreach (var range in ranges.Values)
            {
                var currentVlaue = range.From;

                do
                {
                    yield return currentVlaue;
                    currentVlaue++;
                } while (currentVlaue <= range.To);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}