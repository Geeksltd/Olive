namespace Olive.Entities
{
    public class FullTextSearchQueryOption : QueryOption
    {
        /// <summary>
        /// Creates a new FullTextIndexQueryOption instance.
        /// </summary>
        internal FullTextSearchQueryOption() { }

        #region Keyword
        /// <summary>
        /// Gets or sets the Keywords of this FullTextIndexQueryOption.
        /// </summary>
        public string Keyword { get; set; }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the Properties of this FullTextIndexQueryOption.
        /// </summary>
        public IEnumerable<string> Properties { get; set; }
        #endregion
    }
}
