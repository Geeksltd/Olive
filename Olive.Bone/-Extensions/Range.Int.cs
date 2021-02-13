using System;
using System.Collections.Generic;

namespace Olive
{
    partial class OliveExtensions
    {
        static readonly Random Random = new Random();

        /// <summary>
        /// Gets all possible items in the range based on the specified intervals.
        /// </summary>
        public static IEnumerable<int> GetIntervals(this Range<int> @this, int interval = 1)
        {
            if (interval <= 0) throw new Exception("Interval should be a positive number.");

            for (var item = @this.From; item <= @this.To; item += interval)
                yield return item;
        }

        public static int PickRandom(this Range<int> @this) => Random.Next(@this.From, @this.To);

        public static int GetLength(this Range<int> @this, bool includeEdges) => @this.GetLength() + (includeEdges ? 1 : 0);

        public static Range<int>[] SplitEvenly(this Range<int> @this, int count)
        {
            var rangeLen = @this.GetLength(includeEdges: true);

            if (rangeLen < count) throw new ArgumentOutOfRangeException(nameof(count));

            var result = new Range<int>[count];

            var chunkSize = rangeLen / count;

            for (var counter = 0; counter < count; counter++)
            {
                var lowerBound = @this.From + counter * chunkSize;
                var upperBound = @this.From + (counter + 1) * chunkSize - 1;

                if (counter == count - 1) upperBound = @this.To;

                result[counter] = new Range<int>(lowerBound, upperBound);
            }

            return result;
        }
    }
}