using System;

namespace Olive.Services.Excel
{
    partial class ExcelExporter<T>
    {
        public class ExcelColumn<TColumn>
        {
            public int? Width { get; set; }

            /// <summary>
            /// Gets or sets a Workbook Unique integer ID to use for creating styles.
            /// </summary>
            internal int UniqueId { get; set; }

            public ExcelColumn()
            {
                HeaderStyle = new ExcelCellStyle { BackgroundColor = "#DDDDDD", BorderWidth = 1, BorderColor = "#aaaaaa" };
                GroupingStyle = new ExcelCellStyle { BackgroundColor = "#777777", ForeColor = "#ffffff", Bold = true, Alignment = ExcelExporter.HorizentalAlignment.Center };
                RowStyle = new ExcelCellStyle();
            }

            /// <summary>
            /// Creates a new ExcelColumn instance.
            /// </summary>
            public ExcelColumn(string headerText, string dataType)
                : this()
            {
                HeaderText = headerText;
                DataType = dataType;
            }

            /// <summary>
            /// Sets the specified row style attribute.
            /// </summary>
            public ExcelColumn<TColumn> SetRowStyle(Action<ExcelCellStyle> setter)
            {
                setter?.Invoke(RowStyle);
                return this;
            }

            /// <summary>
            /// Sets the specified header style attribute.
            /// </summary>
            public ExcelColumn<TColumn> SetHeaderStyle(Action<ExcelCellStyle> setter)
            {
                setter?.Invoke(HeaderStyle);
                return this;
            }

            /// <summary>
            /// Gets or sets the style of this columns's header cell.
            /// </summary>
            public ExcelCellStyle HeaderStyle { get; set; }

            public ExcelCellStyle GroupingStyle { get; set; }

            /// <summary>
            /// Gets or sets the style of this columns's data cells.
            /// </summary>
            public ExcelCellStyle RowStyle { get; set; }

            /// <summary>
            /// Gets or sets the HeaderText of this ExcelColumn.
            /// </summary>
            public string HeaderText { get; set; }

            /// <summary>
            /// Gets or sets the Type of this ExcelColumn.
            /// </summary>
            public string DataType { get; set; } = "String";

            /// <summary>
            /// Gets or sets the Formula of this ExcelColumn.
            /// </summary>
            public string Formula { get; set; }

            /// <summary>
            /// Gets or sets the group name of this ExcelColumn.
            /// </summary>
            public string GroupName { get; set; }

            public Func<TColumn, object> Data { get; set; }

            /// <summary>
            /// Customizes this column.
            /// </summary>
            public ExcelColumn<TColumn> Customize(Action<ExcelColumn<TColumn>> customisations)
            {
                customisations?.Invoke(this);
                return this;
            }
        }
    }
}