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
    public class RangeCollection<T> : IEnumerable<T> where T : IComparable, IComparable<T>
    {
        SortedList<T, Range<T>> ranges = new SortedList<T, Range<T>>();

        [NonSerialized]
        Func<T, T> getNextItem, getPreviousItem;

        public RangeCollection() { }

        public override string ToString()
        {
            return ranges.Select(x => x.Value.From.Equals(x.Value.To) ? x.Value.From.ToString() : x.Value.ToString("-")).ToString("|");
        }

        public static RangeCollection<T> Parse(string text)
        {
            var ranges = text.OrEmpty().Split('|').Trim().Select(r =>
            {
                var parts = r.Split('-');
                var from = parts[0].To<T>();
                if (parts.IsSingle()) return new Range<T>(from, from);
                return new Range<T>(from, parts[1].To<T>());
            });

            return new RangeCollection<T>(ranges);
        }

        public RangeCollection(IEnumerable<Range<T>> ranges)
        {
            if (ranges == null) return;

            var array = ranges.ToArray();

            for (var index = 0; index < array.Length; index++)
                for (var inner = index + 1; inner < array.Length; inner++)
                    if (array[index].Intersects(array[inner]))
                        throw new ArgumentException("The initializing ranges should not intersect each other.");

            ranges.Do(r => this.ranges.Add(r.From, r));
        }

        static T Invoke(Func<T, T> method, T arg)
        {
            if (method == null) throw new InvalidOperationException("Initialize() method is not called");
            return method(arg);
        }

        T GetNextItem(T item) => Invoke(getNextItem, item);

        T GetPreviousItem(T item) => Invoke(getPreviousItem, item);

        public Range<T>[] Ranges => ranges.Values.ToArray();

        public void Initialize(Func<T, T> getNextItem, Func<T, T> getPreviousItem)
        {
            this.getNextItem = getNextItem ?? throw new ArgumentNullException(nameof(getNextItem));
            this.getPreviousItem = getPreviousItem ?? throw new ArgumentNullException(nameof(getPreviousItem));
        }

        public void Add(T item)
        {
            var search = DetectRange(item);
            if (search.ExistingRangeStartingWith.HasValue) return;

            if (search.WhereItBelongs == 0) AppendToStart(item);
            else AppendAt(item, search.WhereItBelongs);
        }

        void AppendToStart(T item)
        {
            if (ranges.Any())
            {
                var firstRange = ranges.Values.First();
                var nextItem = GetNextItem(item);

                var canJustExtendTheRange = firstRange.From.Equals(nextItem);

                if (canJustExtendTheRange)
                {
                    ExtendFromLowerBound(item, firstRange);
                    return;
                }
            }

            AddAsNewRange(item);
        }

        Range<T> GetRange(int index) => (index >= 0 && index < ranges.Count) ? ranges.Values[index] : null;

        void AppendAt(T item, int index)
        {
            var range = GetRange(index);
            if (range?.Contains(item) == true) return;

            var previousItem = GetPreviousItem(item);

            if (range?.To?.Equals(previousItem) == true)
            {
                ExtendFromUpperBound(item, range);
                return;
            }

            if (index + 1 < ranges.Count)
            {
                range = ranges.Values[index + 1];

                if (range.From.Equals(GetNextItem(item)))
                {
                    ExtendFromLowerBound(item, range);
                    return;
                }
            }

            AddAsNewRange(item);
        }

        public void Clear() => ranges.Clear();

        public bool Contains(T item)
        {
            var index = ranges.Keys.ToList().BinarySearch(item);
            if (index >= 0) return true;

            // Item is not found with exact index            
            for (var i = (-index) - 1; i >= 0 && i < ranges.Count; i--)
            {
                var range = ranges.Values[i];
                if (range.Contains(item)) return true;
                if (range.To.CompareTo(item) == -1) return false;
            }

            return false;
        }

        public bool Lacks(T item) => !Contains(item);

        public bool Remove(T item)
        {
            var search = DetectRange(item);

            // The item equals one of ranges` from value;
            if (search.ExistingRangeStartingWith.HasValue)
            {
                ShrinkFromLowerBound(item, search.ExistingRangeStartingWith.Value);
                return true;
            }

            if (search.WhereItBelongs == 0) return false;

            var range = GetRange(search.WhereItBelongs);
            if (range == null || range.Lacks(item)) return false;

            if (range.To.Equals(item))
                ShrinkFromUpperBound(range);
            else
                Split(item, range);

            return true;
        }

        void Split(T item, Range<T> range)
        {
            var newRange1 = new Range<T>(range.From, GetPreviousItem(item));
            var newRange2 = new Range<T>(GetNextItem(item), range.To);

            ranges.Remove(range.From);
            ranges.Add(newRange1.From, newRange1);
            ranges.Add(newRange2.From, newRange2);
        }

        void ShrinkFromUpperBound(Range<T> range)
        {
            var newRange = new Range<T>(range.From, GetPreviousItem(range.To));
            ranges.Remove(range.From);
            ranges.Add(newRange.From, newRange);
        }

        void ShrinkFromLowerBound(T item, int index)
        {
            var range = ranges.Values[index];
            ranges.Remove(item);

            if (range.From.Equals(range.To)) return;

            var newRange = new Range<T>(GetNextItem(item), range.To);
            ranges.Add(newRange.From, newRange);
        }

        void AddAsNewRange(T item) => ranges.Add(item, new Range<T>(item, item));

        void ExtendFromUpperBound(T item, Range<T> range)
        {
            var newRange = new Range<T>(range.From, item);

            ranges.Remove(range.From);
            ranges.Add(newRange.From, newRange);
        }

        void ExtendFromLowerBound(T item, Range<T> range)
        {
            var newRange = new Range<T>(item, range.To);

            ranges.Remove(range.From);
            ranges.Add(newRange.From, newRange);
        }

        class RangeIndexSearchResult
        {
            public int? ExistingRangeStartingWith;
            public int WhereItBelongs;
        }

        RangeIndexSearchResult DetectRange(T item)
        {
            var index = ranges.Keys.ToList().BinarySearch(item);
            if (index >= 0) return new RangeIndexSearchResult { ExistingRangeStartingWith = index };
            else return new RangeIndexSearchResult { WhereItBelongs = (-index) - 1 };
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var range in ranges.Values)
            {
                var currentVlaue = range.From;

                do
                {
                    yield return currentVlaue;

                    currentVlaue = GetNextItem(currentVlaue);
                } while (currentVlaue.CompareTo(range.To) <= 0);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}