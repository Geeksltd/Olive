namespace Olive.Export
{
    class DropDownColumn<TColumn> : Column<TColumn>
    {
        public DropDownColumn(string headerText, string dataType, string enumerationName, object[] possibleValues)
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