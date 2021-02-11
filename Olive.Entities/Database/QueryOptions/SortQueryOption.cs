namespace Olive.Entities
{
    public class SortQueryOption : QueryOption
    {
        /// <summary>
        /// Gets or sets the Property of this SortQueryOption.
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the Descending of this SortQueryOption.
        /// </summary>
        public bool Descending { get; set; }
    }
}
