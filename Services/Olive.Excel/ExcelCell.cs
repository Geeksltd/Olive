using System;

namespace Olive.Excel
{
    public class ExcelCell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelCell"/> class.
        /// </summary>
        public ExcelCell() => Style = new ExcelCellStyle();

        public ExcelCell(string text) : this() => Text = text;

        public ExcelCell SetStyle(Action<ExcelCellStyle> setter)
        {
            setter?.Invoke(Style);
            return this;
        }

        /// <summary>
        /// Gets or sets the text of this cell.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the type of this cell.
        /// </summary>
        public string Type { get; set; }

        public ExcelCellStyle Style { get; set; }

        /// <summary>
        /// Determines if this cell has the same style as the specifying one.
        /// </summary>
        internal bool MatchStyle(ExcelCell other) => Style == other.Style;

        public override string ToString() => Text;
    }
}