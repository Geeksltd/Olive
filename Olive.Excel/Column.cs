using System;

namespace Olive.Export
{
    public class Column<TColumn>
    {
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets a Workbook Unique integer ID to use for creating styles.
        /// </summary>
        internal int UniqueId { get; set; }

        public Column()
        {
            HeaderStyle = new CellStyle
            {
                BackgroundColor = "#DDDDDD",
                BorderWidth = 1,
                BorderColor = "#aaaaaa"
            };

            GroupingStyle = new CellStyle
            {
                BackgroundColor = "#777777",
                ForeColor = "#ffffff",
                Bold = true,
                Alignment = Exporter.HorizentalAlignment.Center
            };

            RowStyle = new CellStyle();
        }

        public Column(string headerText, string dataType) : this()
        {
            HeaderText = headerText;
            DataType = dataType;
        }

        public Column<TColumn> SetRowStyle(Action<CellStyle> setter)
        {
            setter?.Invoke(RowStyle);
            return this;
        }

        public Column<TColumn> SetHeaderStyle(Action<CellStyle> setter)
        {
            setter?.Invoke(HeaderStyle);
            return this;
        }

        public CellStyle HeaderStyle { get; set; }

        public CellStyle GroupingStyle { get; set; }

        public CellStyle RowStyle { get; set; }

        public string HeaderText { get; set; }

        public string DataType { get; set; } = "String";

        public string Formula { get; set; }

        public string GroupName { get; set; }

        public Func<TColumn, object> Data { get; set; }

        public Column<TColumn> Customize(Action<Column<TColumn>> customisations)
        {
            customisations?.Invoke(this);
            return this;
        }
    }
}