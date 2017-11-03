namespace Olive.Services.Excel
{
    partial class ExcelExporter<T>
    {
        class ExcelDropDownColumn<TColumn> : ExcelColumn<TColumn>
        {
            public ExcelDropDownColumn(string headerText, string dataType, string enumerationName, object[] possibleValues)
                : base(headerText, dataType)
            {
                PossibleValues = possibleValues;
                EnumerationName = enumerationName;
            }

            /// <summary>
            /// enumeration items to select from
            /// </summary>
            public object[] PossibleValues { get; set; }

            /// <summary>
            /// Gets or sets the Name of this ExcelDropDownColumn.
            /// </summary>
            public string EnumerationName { get; set; }
        }
    }
}