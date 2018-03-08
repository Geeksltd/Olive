using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olive.Export
{
    /// <summary>
    /// Provides styles for export cells.
    /// </summary>
    public class CellStyle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CellStyle" /> class.
        /// </summary>
        public CellStyle() => Italic = false;

        #region Alignment

        /// <summary>
        /// Gets or sets the horizontal alignment of this style.
        /// </summary>
        public Exporter.HorizentalAlignment Alignment
        {
            get => GetSetting("Alignment.Horizontal", Exporter.HorizentalAlignment.Left);
            set => Settings["Alignment.Horizontal"] = ((int)value).ToString();
        }

        #endregion

        #region VerticalAlignment

        /// <summary>
        /// Gets or sets the vertical alignment of this style.
        /// </summary>
        public Exporter.VerticalAlignment VerticalAlignment
        {
            get => GetSetting("Alignment.Vertical", Exporter.VerticalAlignment.Center);
            set => Settings["Alignment.Vertical"] = ((int)value).ToString();
        }

        #endregion

        #region Orientation

        /// <summary>
        /// Gets or sets the cell orientation of this style.
        /// </summary>
        public Exporter.CellOrientation Orientation
        {
            get => GetSetting("Alignment.Orientation", Exporter.CellOrientation.Horizontal);
            set => Settings["Alignment.Orientation"] = ((int)value).ToString();
        }

        #endregion

        #region FontSize

        /// <summary>
        /// Gets or sets the size of the font.
        /// </summary>        
        public int FontSize
        {
            get => GetSetting("Font.FontSize", 10);
            set => Settings["Font.FontSize"] = value.ToString();
        }

        #endregion

        #region BackgroundColor

        /// <summary>
        /// Gets or sets the background color of this style.
        /// </summary>
        public string BackgroundColor
        {
            get => Settings.TryGet("Interior.Color").Or("#ffffff");
            set => Settings["Interior.Color"] = value;
        }

        #endregion

        #region Border Color

        /// <summary>
        /// Gets or sets the border color of this style.
        /// </summary>
        public string BorderColor
        {
            get => Settings.TryGet("Border.Color").Or("#000000");
            set => Settings["Border.Color"] = value;
        }

        #endregion

        #region BorderWidth

        /// <summary>
        /// Gets or sets the width of the border.
        /// </summary>        
        public int BorderWidth
        {
            get => Settings.TryGet("Border.Width").TryParseAs<int>() ?? 0;
            set
            {
                if (value < 0 || value > 2) throw new Exception("Border width should be 0, 1 or 2");
                Settings["Border.Width"] = value.ToString();
            }
        }

        #endregion

        #region FontName

        /// <summary>
        /// Gets or sets the font name of this style.
        /// </summary>
        public string FontName
        {
            get => Settings.TryGet("Font.FontName").Or("Arial");
            set => Settings["Font.FontName"] = value;
        }

        #endregion

        #region NumberFormat

        /// <summary>
        /// Gets or sets the Number format of this style.
        /// </summary>
        public string NumberFormat
        {
            get => Settings.TryGet("NumberFormat.Format");
            set => Settings["NumberFormat.Format"] = value;
        }

        #endregion

        #region Bold

        /// <summary>
        /// Gets or sets if font should be bold.
        /// </summary>
        public bool Bold
        {
            get => Settings.TryGet("Font.Bold").TryParseAs<bool>() ?? false;
            set => Settings["Font.Bold"] = value.ToString();
        }

        #endregion

        #region WrapText

        /// <summary>
        /// Gets or sets if the text should be wrapped.
        /// </summary>
        public bool WrapText
        {
            get => Settings.TryGet("WrapText").TryParseAs<bool>() ?? true;
            set => Settings["WrapText"] = value.ToString();
        }

        #endregion

        #region Italic

        /// <summary>
        /// Gets or sets if font should be Italic.
        /// </summary>
        public bool Italic
        {
            get => Settings.TryGet("Font.Italic").TryParseAs<bool>() ?? false;
            set => Settings["Font.Italic"] = value.ToString();
        }

        #endregion

        #region ForeColor

        /// <summary>
        /// Gets or sets the background color of this style.
        /// </summary>
        public string ForeColor
        {
            get => Settings.TryGet("Font.Color").Or("#000000");
            set => Settings["Font.Color"] = value;
        }

        #endregion

        #region Manage Style items

        /// <summary>
        /// Gets or sets the settings.
        /// Use Exporter.Style.[Item] to add styles to this.
        /// </summary>
        public Dictionary<string, string> Settings = new Dictionary<string, string>();

        /// <summary>
        /// Use Exporter.Style.[Item] to add styles.
        /// </summary>
        public CellStyle Set(string key, string value)
        {
            Settings[key] = value;
            return this;
        }

        #endregion

        T GetSetting<T>(string settingKey, T defaultValue) =>
            (T)(object)(Settings.TryGet(settingKey).TryParseAs<int>() ?? (int)(object)defaultValue);

        public override bool Equals(object obj)
        {
            var style2 = obj as CellStyle;
            if (style2 == null) return false;

            if (ReferenceEquals(this, style2)) return true;

            if (((object)this == null) || ((object)style2 == null)) return false;

            return new[] {
                new { Value1 = BackgroundColor , Value2 = style2.BackgroundColor },
                new { Value1 = FontName , Value2 = style2.FontName },
                new { Value1 = ForeColor , Value2 = style2.ForeColor },
            }
            .All(s => s.Value2?.ToString().ToLower() == s.Value1.ToLowerOrEmpty());
        }

        public static bool operator ==(CellStyle style1, CellStyle style2)
        {
            if (ReferenceEquals(style1, style2)) return true;

            if ((object)style1 == null) return false;

            return style1.Equals(style2);
        }

        public static bool operator !=(CellStyle style1, CellStyle style2) => !(style1 == style2);

        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Gets a unique ID for this style.
        /// </summary>
        public string GetStyleId() => "s" + Settings.Select(i => "s" + i.Key + "_" + i.Value).ToString("__").GetHashCode();

        internal string GenerateStyle() => GenerateStyleTemplate().Replace("[#Style.ID#]", GetStyleId());

        string GenerateStyleTemplate()
        {
            var r = new StringBuilder();

            r.AppendLine(@"<Style ss:ID=""[#Style.ID#]"">");
            r.AddFormattedLine(@"<Alignment ss:Horizontal=""{0}"" ss:Vertical=""{1}"" ss:Rotate=""{2}""{3}/>", Alignment, VerticalAlignment, GetCellRotation(), " ss:WrapText=\"1\"".OnlyWhen(WrapText));
            r.AddFormattedLine(@"<Font ss:FontName=""{0}"" x:Family=""Swiss"" ss:Size=""{1}"" ss:Color=""{2}"" ss:Bold=""{3}"" ss:Italic=""{4}"" />", FontName, FontSize, ForeColor, Bold ? 1 : 0, Italic ? 1 : 0);

            if (BackgroundColor.HasValue() && BackgroundColor.ToUpper() != "#FFFFFF")
            {
                r.AddFormattedLine(@"<Interior ss:Color=""{0}"" ss:Pattern=""Solid""/>", BackgroundColor);
            }

            if (BorderWidth > 0)
            {
                r.AddFormattedLine(@"<Borders>
                                  <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""{1}"" ss:Color=""{0}""/>
                                  <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""{1}"" ss:Color=""{0}""/>
                                  <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""{1}"" ss:Color=""{0}""/>
                                  <Border ss:Position=""Top"" ss:LineStyle=""Continuous""  ss:Weight=""{1}"" ss:Color=""{0}""/>
                                 </Borders>", BorderColor, BorderWidth);
            }

            if (NumberFormat.HasValue())
                r.AddFormattedLine(@"<NumberFormat ss:Format=""{0}"" />", NumberFormat.HtmlEncode());

            r.AppendLine(@"</Style>");

            return r.ToString();
        }

        string GetCellRotation()
        {
            switch (Orientation)
            {
                case Exporter.CellOrientation.Vertical:
                    return "90";
                case Exporter.CellOrientation.Horizontal:
                    return "0";
                default:
                    throw new NotSupportedException("This orientation is not supported.");
            }
        }

        internal CellStyle OverrideWith(CellStyle overrideStyle)
        {
            var result = new CellStyle();
            result.Settings.Add(Settings);

            foreach (var setting in overrideStyle.Settings)
                result.Settings[setting.Key] = setting.Value;

            return result;
        }
    }
}