using System;

namespace Olive.Entities
{
    public class PagingQueryOption : QueryOption
    {
        /// <summary>
        /// Creates a new ResultSetSizeQueryOption instance.
        /// </summary>
        internal PagingQueryOption() { }

        /// <summary>
        /// Creates a new ResultSetSizeQueryOption instance.
        /// </summary>
        public PagingQueryOption(string orderBy, int startIndex, int pageSize)
        {
            if (orderBy.IsEmpty())
                throw new ArgumentException("Invalid PagingQueryOption specified. OrderBy is mandatory.");

            if (pageSize < 1)
                throw new ArgumentException("Invalid PagingQueryOption specified. PageSize should be a positive number.");

            OrderBy = orderBy;
            PageSize = pageSize;
            PageStartIndex = startIndex;
        }

        public int PageStartIndex { get; internal set; }

        public int PageSize { get; internal set; }

        /// <summary>
        /// The direct SQL sort expression. E.g: MyColumn DESC, Something 
        /// </summary>
        public string OrderBy { get; set; }
    }
}