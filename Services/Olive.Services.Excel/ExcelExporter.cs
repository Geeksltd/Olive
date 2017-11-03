namespace Olive.Services.Excel
{
    public partial class ExcelExporter : ExcelExporter<object>
    {
        /// <summary>
        /// Creates a new ExcelExporter instance.
        /// </summary>
        public ExcelExporter(string documentName) : base(documentName) { }

        /// <summary>
        /// Creates a new ExcelExporter instance.
        /// </summary>
        public ExcelExporter(System.Data.DataTable dataTable) : base(dataTable) { }

        public enum CellOrientation : int { Vertical, Horizontal }
        public enum VerticalAlignment : int { Top, Center, Bottom }
        public enum HorizentalAlignment : int { Left, Center, Right }

        public class Styles
        {
            public const string NumberFormat_Format = "NumberFormat.Format";
            public const string Alignment_Vertical = "Alignment.Vertical";
            public const string Alignment_Horizontal = "Alignment.Horizontal";

            public const string Font_FontName = "Font.FontName";
            public const string Font_Bold = "Font.Bold";
            public const string Font_Italic = "Font.Italic";
            public const string Font_Size = "Font.Size";
            public const string Font_Color = "Font.Color";

            public const string Interior_Color = "Interior.Color";
            public const string Interior_Pattern = "Interior.Pattern";
        }
    }
}