namespace Olive.Entities
{
    public class TakeTopQueryOption : QueryOption
    {
        /// <summary>
        /// Creates a new ResultSetSizeQueryOption instance.
        /// </summary>
        public TakeTopQueryOption(int number) => Number = number;

        /// <summary>
        /// Gets or sets the Number of this ResultSetSizeQueryOption.
        /// </summary>
        public int Number { get; }
    }
}