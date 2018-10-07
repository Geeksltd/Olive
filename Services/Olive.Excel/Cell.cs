using System;

namespace Olive.Export
{
    public class Cell
    {
        public Cell(string text = null) => Text = text;

        public Cell SetStyle(Action<CellStyle> setter)
        {
            setter?.Invoke(Style);
            return this;
        }

        public string Text { get; set; }

        public string Type { get; set; }

        public CellStyle Style { get; set; } = new CellStyle();

        /// <summary>
        /// Determines if this cell has the same style as the specifying one.
        /// </summary>
        internal bool MatchStyle(Cell other) => Style == other.Style;

        public override string ToString() => Text;
    }
}