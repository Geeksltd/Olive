using System;

namespace Olive
{
    /// <summary>
    /// Provides a range of values.
    /// </summary>
    [Serializable]
    public class Range<T> where T : IComparable, IComparable<T>
    {
        const int NINETEEN_HUNDRED = 1900;

        /// <summary>
        /// Gets or sets the From of this Range.
        /// </summary>
        public T From { get; set; }

        /// <summary>
        /// Gets or sets the To of this Range.
        /// </summary>
        public T To { get; set; }

        /// <summary>
        /// Creates a new Range instance.
        /// </summary>
        public Range() { }

        /// <summary>
        /// Creates a new Range instance with the specified boundaries.
        /// </summary>
        public Range(T from, T to)
        {
            if (from.CompareTo(to) > 0)
                throw new ArgumentException($"nameof{from} should be smaller than or equal to {nameof(To)} value in a range.");

            From = from;
            To = to;
        }

        /// <summary>
        /// Gets the length of this range. For a date range, use the TimeOfDay property of the returned date time.
        /// </summary>
        public T GetLength()
        {
            if (From.CompareTo(To) > 0)
                throw new InvalidOperationException("'from' should be smaller than or equal to 'To' value in a range.");

            var fromValue = (object)From;
            var toValue = (object)To;
            object result;

            if (typeof(T) == typeof(int)) result = (int)toValue - (int)fromValue;
            else if (typeof(T) == typeof(double)) result = (double)toValue - (double)fromValue;
            else if (typeof(T) == typeof(long)) result = (long)toValue - (long)fromValue;
            else if (typeof(T) == typeof(decimal)) result = (decimal)toValue - (decimal)fromValue;
            else if (typeof(T) == typeof(DateTime))
                result = new DateTime(NINETEEN_HUNDRED, 1, 1).Add((DateTime)toValue - (DateTime)fromValue);
            else
                throw new NotSupportedException("GetLength() is not supported on type: " + typeof(T).FullName);

            return (T)result;
        }

        /// <summary>
        /// Determines whether or not this range lacks the given value.
        /// </summary>
        public bool Lacks(T value, bool includingEdges = true) => !Contains(value, includingEdges);

        /// <summary>
        /// Determines whether or not this range cotnains the given value
        /// </summary>
        public bool Contains(T value, bool includeEdges = true)
        {
            if (includeEdges)
            {
                if (value.CompareTo(From) < 0) return false;
                if (value.CompareTo(To) > 0) return false;
            }
            else
            {
                if (value.CompareTo(From) <= 0) return false;
                if (value.CompareTo(To) >= 0) return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether or not this range contains with the given range
        /// </summary>
        public bool Contains(Range<T> range) => Contains(range.From, includeEdges: true) && Contains(range.To, includeEdges: true);

        /// <summary>
        /// Determines whether or not this range intersects with the given range
        /// </summary>
        public bool Intersects(Range<T> range, bool includeEdges = true)
        {
            return Contains(range.From, includeEdges) ||
                Contains(range.To, includeEdges) ||
                range.Contains(From, includeEdges) ||
                range.Contains(To, includeEdges) ||
                (From.Equals(range.From) && To.Equals(range.To));
        }

        /// <summary>
        /// Returns: {From} - {To}.
        /// </summary>
        public override string ToString() => ToString(" - ");

        /// <summary>
        /// Returns {From}{rangeSeparator}{To}.
        /// </summary>
        public string ToString(string rangeSeparator) => From + rangeSeparator + To;

        /// <summary>
        /// Returns the From and To values formatted by the specified format and then joined together with the specified rangeSeparator.
        /// </summary>
        /// <param name="perItemFormat">E.g. {0:dd MM yy}</param>
        public string ToString(string rangeSeparator, string perItemFormat) =>
            string.Format(perItemFormat, From) + rangeSeparator + string.Format(perItemFormat, To);
    }
}