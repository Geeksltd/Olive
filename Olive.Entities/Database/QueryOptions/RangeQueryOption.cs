namespace Olive.Entities
{
    public class RangeQueryOption : QueryOption
    {
        /// <summary>
        /// Creates a new ResultSetSizeQueryOption instance.
        /// </summary>
        internal RangeQueryOption() { }

        public int From { get; internal set; }

        public int Number { get; internal set; }
    }
}
