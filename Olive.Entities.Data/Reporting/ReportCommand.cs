namespace Olive.Entities.Data
{
    /// <summary>The parts of a report query's SELECT, each already in the provider's SQL, for
    /// <see cref="SqlCommandGenerator.GenerateReportCommand"/> to put together.</summary>
    public class ReportCommand
    {
        /// <summary>Whether to read each distinct row once.</summary>
        public bool Distinct { get; set; }

        /// <summary>The select list, each column with its alias.</summary>
        public string Columns { get; set; }

        /// <summary>The FROM clause's content: the table with its alias, then its joins.</summary>
        public string Tables { get; set; }

        /// <summary>The WHERE clause, starting " WHERE ".</summary>
        public string Where { get; set; }

        /// <summary>The GROUP BY list, or empty.</summary>
        public string GroupBy { get; set; }

        /// <summary>The HAVING condition, or empty.</summary>
        public string Having { get; set; }

        /// <summary>The ORDER BY list, or empty.</summary>
        public string OrderBy { get; set; }

        /// <summary>How many rows to read at most, when there is no paging.</summary>
        public int? Top { get; set; }

        /// <summary>The first row of the page, when <see cref="PageSize"/> is set.</summary>
        public int PageStartIndex { get; set; }

        /// <summary>How many rows a page holds, or null for no paging.</summary>
        public int? PageSize { get; set; }

        /// <summary>The clauses after FROM that every provider spells the same.</summary>
        public string Clauses =>
            Where + GroupBy.WithPrefix(" GROUP BY ") + Having.WithPrefix(" HAVING ") + OrderBy.WithPrefix(" ORDER BY ");
    }
}
