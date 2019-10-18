using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Olive
{
#pragma warning disable GCop112 // This class is too large. Break its responsibilities down into more classes.
    partial class OliveExtensions
#pragma warning restore GCop112 // This class is too large. Break its responsibilities down into more classes.
    {
        static byte UTF8SignatureFirstByte = 0xEF;
        static byte UTF8SignatureSecondByte = 0xBB;
        static byte UTF8SignatureThirdByte = 0xBF;

        static string[][] XMLEscapingChars = new string[][]
        {
            new string[]{ "&",  "&amp;" },
            new string[]{ "<",  "&lt;" },
            new string[]{ ">",  "&gt;" },
            new string[]{ "\"", "&quot;" },
            new string[]{ "'",  "&apos;" },
        };

        /// <summary>
        /// Array of unsafe characters that need to be replaced with their character code literals in a JavaScript string.
        /// </summary>
        static readonly char[] JsUnsafeCharacters = new[] { '\'', '\"' };

        static ConcurrentDictionary<string, string> LiteralFromPascalCaseCache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Removes the specified text from the start of this string instance.
        /// </summary>
        /// <param name="textToTrim">Determines the string which removed if it is occured in start of this string.</param>
	    public static string TrimStart(this string @this, string textToTrim)
        {
            @this = @this.OrEmpty();

            if (textToTrim.IsEmpty() || @this.IsEmpty()) return @this;

            if (@this.StartsWith(textToTrim)) return @this.Substring(textToTrim.Length).TrimStart(textToTrim);
            else return @this;
        }

        /// <summary>
        /// Trims the end of this instance of string with the specified number of characters.
        /// </summary>
        /// <param name="numberOfCharacters">The specified number of characters which removed from end of this string.</param>
        public static string TrimEnd(this string @this, int numberOfCharacters)
        {
            if (numberOfCharacters < 0)
                throw new ArgumentException("numberOfCharacters must be greater than 0.");

            if (numberOfCharacters == 0) return @this;

            if (@this.IsEmpty() || @this.Length <= numberOfCharacters)
                return string.Empty;

            return @this.Substring(0, @this.Length - numberOfCharacters);
        }

        /// <summary>
        /// If this string object is null, it will return null. Otherwise it will trim the text and return it.
        /// </summary>
        public static string TrimOrNull(this string @this) => @this?.Trim();

        /// <summary>
        /// If this string object is null, it will return empty string. Otherwise it will trim the text and return it.
        /// </summary>
        public static string TrimOrEmpty(this string @this) => @this.TrimOrNull().OrEmpty();

        /// <summary>
        /// If this string object is null, it will return empty. Otherwise it will trim the text and return it.
        /// </summary>
        /// <param name="items">The list of items which are compared to this string.</param>
        public static bool IsNoneOf(this string @this, params string[] items) => !@this.IsAnyOf(items);

        /// <summary>
        /// Returns a copy of this text converted to lower case. If it is null it will return empty string.
        /// </summary>
        public static string ToLowerOrEmpty(this string @this) => @this.OrEmpty().ToLower();

        /// <summary>
        /// Returns a copy of this text converted to upper case. If it is null it will return empty string.
        /// </summary>
        public static string ToUpperOrEmpty(this string @this) => @this.OrEmpty().ToUpper();

        /// <summary>
        /// Determines this value is one of the {Args}.
        /// </summary>
        /// <param name="items">The list of items which are compared to this string.</param>
        public static bool IsAnyOf(this string @this, params string[] items) => items.Contains(@this);

        /// <summary>
        /// Determines this value is one of the {Args}.
        /// </summary>
        /// <param name="items">The list of items which are compared to this string.</param>
        public static bool IsAnyOf(this string @this, IEnumerable<string> items)
            => IsAnyOf(@this, items.ToArray());

        /// <summary>
        /// If this string have one of the {Args} parameter, it returns true, otherwise it returns false.
        /// </summary>
        /// <param name="keywords">The list of items which are checked.</param>
        /// <param name="caseSensitive">determines whether sensitivity is checked or not. Default value is false.</param>
        public static bool ContainsAll(this string @this, string[] keywords, bool caseSensitive)
        {
            if (!caseSensitive)
            {
                @this = @this.ToLowerOrEmpty();

                for (var i = 0; i < keywords.Length; i++) keywords[i] = keywords[i].ToLower();
            }

            foreach (var key in keywords)
                if (!@this.Contains(key)) return false;

            return true;
        }

        /// <summary>
        /// Determines whether this instance of string is null or empty.
        /// </summary>
        [EscapeGCop("I am the solution myself!")]
        public static bool IsEmpty(this string text) => string.IsNullOrEmpty(text);

        /// <summary>
        /// Determines whether this instance of string is not null or empty.
        /// </summary>
        [EscapeGCop("I am the solution myself!")]
        public static bool HasValue(this string text) => !string.IsNullOrEmpty(text);

        /// <summary>
        /// Will replace all line breaks with a BR tag and return the result as a raw html.
        /// </summary>
        public static string ToHtmlLines(this string @this) => @this.OrEmpty().ToLines().ToString("<br/>");

        /// <summary>
        /// Will join all items with a BR tag and return the result as a raw html.
        /// </summary>
        public static string ToHtmlLines<T>(this IEnumerable<T> @this) => @this.ToString("<br/>");

        /// <summary>
        /// Will join all items with a BR tag and return the result as a raw html.
        /// </summary>
        public static Task<string> ToHtmlLines<T>(this Task<IEnumerable<T>> @this) => @this.ToString("<br/>");

        /// <summary>
        /// Gets the same string if it is not null or empty. Otherwise it returns the specified default value.
        /// </summary>
        public static string Or(this string @this, string defaultValue)
        {
            if (@this.IsEmpty()) return defaultValue;
            else return @this;
        }

        /// <summary>
        /// Gets the same string if it is not null or empty. Otherwise it returns the specified default value.
        /// </summary>
        /// <param name="defaultValueProvider">It is returned if this string is Null.</param>
        public static string Or(this string @this, Func<string> defaultValueProvider)
        {
            if (@this.IsEmpty()) return defaultValueProvider?.Invoke();
            else return @this;
        }

        /// <summary>
        /// Gets the same string unless it is the same as the specified text. If they are the same, empty string will be returned.
        /// </summary>
        /// <param name="unwantedText">The string is used to search in this string.</param>
        public static string Unless(this string @this, string unwantedText)
        {
            if (@this == unwantedText) return string.Empty;
            else return @this;
        }

        /// <summary>
        /// Gets the same string unless it is the same as the specified text. If they are the same, empty string will be returned.
        /// </summary>
        /// <param name="unwantedText">The string is used to search in this string.</param>
        /// <param name="caseSensitive">Determines whether caseSensitive is important or not. Default value is True.</param>
        public static string Unless(this string @this, string unwantedText, bool caseSensitive)
        {
            if (caseSensitive) return @this.Unless(unwantedText);

            if (@this.IsEmpty()) return string.Empty;
            else if (@this.Equals(unwantedText, StringComparison.OrdinalIgnoreCase)) return string.Empty;
            else return @this;
        }

        /// <summary>
        /// Summarizes the specified source.
        /// </summary>
        /// <param name="maximumLength">The number of characters which should be shown. It should be greater than 3 if the enforceMaxLength parameter is true.</param>
        /// <param name="enforceMaxLength">Determines whether maximumLength parameter should be enforced or not.</param>
        public static string Summarize(this string @this, int maximumLength, bool enforceMaxLength)
        {
            var result = Summarize(@this, maximumLength);

            if (enforceMaxLength && result.Length > maximumLength)
                result = @this.Substring(0, maximumLength - 3) + "...";

            return result;
        }

        /// <summary>
        /// Summarizes the specified text.
        /// </summary>        
        /// <param name="maximumLength">The number of characters which should be shown.</param>
        public static string Summarize(this string @this, int maximumLength)
        {
            if (@this.IsEmpty()) return @this;

            if (@this.Length > maximumLength)
            {
                @this = @this.Substring(0, maximumLength);

                var lastSpace = -1;

                foreach (var wordSeperator in " \r\n\t")
                    lastSpace = Math.Max(@this.LastIndexOf(wordSeperator), lastSpace);

                if (lastSpace > maximumLength / 2)
                    @this = @this.Substring(0, lastSpace);

                @this += "...";
            }

            return @this;
        }

        #region Count string

        /// <summary>
        /// Returns the number of members of this value.
        /// </summary>        
        /// <param name="objectTitle">Determines the title of the object.</param>
        public static string Count<T>(this IEnumerable<T> @this, string objectTitle)
        {
            if (objectTitle.IsEmpty())
                objectTitle = SeparateAtUpperCases(typeof(T).Name);

            return objectTitle.ToCountString(@this.Count());
        }

        /// <summary>
        /// Returns the number of members of this value.
        /// </summary>        
        /// <param name="objectTitle">Determines the title of the object.</param>
        /// <param name="zeroQualifier">Determines the title when the number of items is zero.</param>
        public static string Count<T>(this IEnumerable<T> @this, string objectTitle, string zeroQualifier)
        {
            if (objectTitle.IsEmpty())
                objectTitle = SeparateAtUpperCases(typeof(T).Name);

            return objectTitle.ToCountString(@this.Count(), zeroQualifier);
        }

        /// <summary>
        /// Inserts a "s" in the end of this string if {count} greater than zero.
        /// </summary>        
        /// <param name="count">Determines the number of this object.</param>
        public static string ToCountString(this string @this, int count)
        {
            var zeroQualifier = "no";

            if (@this.HasValue() && char.IsUpper(@this[0]))
                zeroQualifier = "No";

            return ToCountString(@this, count, zeroQualifier);
        }

        /// <summary>
        /// Inserts a "s" in the end of this string if {count} greater than zero.
        /// </summary>        
        /// <param name="count">Determines the number of this object.</param>
        /// <param name="zeroQualifier">is a string that if the {count} is zero, it is added to the end of the output string.</param>
        public static string ToCountString(this string @this, int count, string zeroQualifier)
        {
            @this = @this.Or("").Trim();
            if (@this.IsEmpty())
                throw new Exception("'name' cannot be empty for ToCountString().");

            if (count < 0)
                throw new ArgumentException("count should be greater than or equal to 0.");

            if (count == 0) return zeroQualifier + " " + @this;
            else if (count == 1) return "1 " + @this;
            else return $"{count} {@this.ToPlural()}";
        }

        /// <summary>
        /// Inserts a space after each uppercase characters in this string.
        /// </summary>        
        public static string SeparateAtUpperCases(this string @this)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < @this.Length; i++)
            {
                if (char.IsUpper(@this[i]) && i > 0)
                    sb.Append(" ");
                sb.Append(@this[i]);
            }

            return sb.ToString().ToLower();
        }

        /// <summary>
        /// Returns the plural form of this word.
        /// </summary>        
        public static string ToPlural(this string @this)
        {
            if (@this.IsEmpty()) return string.Empty;

            // Only change the last word:
            var phrase = @this;
            var prefix = "";
            if (phrase.Split(' ').Length > 1)
            {
                // Multi word, set prefix to anything but the last word:
                prefix = phrase.Substring(0, phrase.LastIndexOf(" ")) + " ";
                @this = phrase.Substring(phrase.LastIndexOf(" ") + 1);
            }

            string plural;
            var irregular = GetIrregularPlural(@this);

            if (irregular.HasValue())
            {
                if (prefix.IsEmpty())
                    irregular = char.ToUpper(irregular[0]) + irregular.Substring(1);

                plural = irregular;
            }
            else plural = GetRegularPlural(@this);

            return prefix + plural;
        }

        static string GetRegularPlural(string singular)
        {
            var ending = char.ToLower(singular[singular.Length - 1]);

            char secondEnding;
            if (singular.Length > 1)
                secondEnding = char.ToLower(singular[singular.Length - 2]);
            else
                secondEnding = char.MinValue;

            if (ending == 'x' || ending == 'z' || ending == 's' || (secondEnding.ToString() + ending) == "ch" || (secondEnding.ToString() + ending) == "sh")
                return singular + "es";

            else if (ItNeedsIESForPlural(ending, secondEnding))
                return singular.Substring(0, singular.Length - 1) + "ies";

            return singular + "s";
        }

        static bool ItNeedsIESForPlural(char ending, char secondEnding)
        {
            return ending == 'y' &&
                secondEnding != 'a' &&
                secondEnding != 'e' &&
                secondEnding != 'o' &&
                secondEnding != 'i' &&
                secondEnding != 'u';
        }

        [EscapeGCop("It is fine for this method to be long.")]
        static string GetIrregularPlural(string singular)
        {
            singular = singular.ToLower();

            switch (singular)
            {
                case "addendum": return "addenda";
                case "alga": return "algae";
                case "alumna": return "alumnae";
                case "alumnus": return "alumni";
                case "analysis": return "analyses";
                case "apparatus": return "apparatuses";
                case "appendix": return "appendices";
                case "axis": return "axes";
                case "bacillus": return "bacilli";
                case "bacterium": return "bacteria";
                case "basis": return "bases";
                case "beau": return "beaux";
                case "bison": return "bison";
                case "buffalo": return "buffaloes";
                case "bureau": return "bureaus";
                case "calf": return "calves";
                case "child": return "children";
                case "corps": return "corps";
                case "crisis": return "crises";
                case "criterion": return "criteria";
                case "curriculum": return "curricula";
                case "datum": return "data";
                case "deer": return "deer";
                case "die": return "dice";
                case "dwarf": return "dwarfs";
                case "diagnosis": return "diagnoses";
                case "echo": return "echoes";
                case "elf": return "elves";
                case "ellipsis": return "ellipses";
                case "embargo": return "embargoes";
                case "emphasis": return "emphases";
                case "erratum": return "errata";
                case "fireman": return "firemen";
                case "fish": return "fish";
                case "focus": return "focus";
                case "foot": return "feet";
                case "formula": return "formulas";
                case "fungus": return "fungi";
                case "genus": return "genera";
                case "goose": return "geese";
                case "half": return "halves";
                case "hero": return "heroes";
                case "hippopotamus": return "hippopotami";
                case "hoof": return "hoofs";
                case "hypothesis": return "hypotheses";
                case "index": return "indices";
                case "knife": return "knives";
                case "leaf": return "leaves";
                case "life": return "lives";
                case "loaf": return "loaves";
                case "louse": return "lice";
                case "man": return "men";
                case "matrix": return "matrices";
                case "means": return "means";
                case "medium": return "media";
                case "memorandum": return "memoranda";
                case "millennium": return "milennia";
                case "moose": return "moose";
                case "mosquito": return "mosquitoes";
                case "mouse": return "mice";
                case "nebula": return "nebulas";
                case "neurosis": return "neuroses";
                case "nucleus": return "nuclei";
                case "oasis": return "oases";
                case "octopus": return "octopi";
                case "ovum": return "ova";
                case "ox": return "oxen";
                case "paralysis": return "paralyses";
                case "parenthesis": return "parentheses";
                case "person": return "people";
                case "phenomenon": return "phenomena";
                case "potato": return "potatoes";
                case "scarf": return "scarfs";
                case "self": return "selves";
                case "series": return "series";
                case "sheep": return "sheep";
                case "shelf": return "shelves";
                case "scissors": return "scissors";
                case "species": return "species";
                case "stimulus": return "stimuli";
                case "stratum": return "strata";
                case "synthesis": return "syntheses";
                case "synopsis": return "synopses";
                case "tableau": return "tableaux";
                case "that": return "those";
                case "thesis": return "theses";
                case "thief": return "thieves";
                case "this": return "these";
                case "tomato": return "tomatoes";
                case "tooth": return "teeth";
                case "torpedo": return "torpedoes";
                case "vertebra": return "vertebrae";
                case "veto": return "vetoes";
                case "vita": return "vitae";
                case "watch": return "watches";
                case "wife": return "wives";
                case "wolf": return "wolves";
                case "woman": return "women";
                case "cactus": return "cacti";
                case "syllabus": return "syllabi";

                default: return "";
            }
        }

        #endregion

        /// <summary>
        /// Trims some unnecessary text from the end of this string, if it exists.
        /// </summary>
        /// <param name="unnecessaryText">Specific number of characters from this value that you want to be removed.</param>
        public static string TrimEnd(this string @this, string unnecessaryText) => TrimEnd(@this, unnecessaryText, caseSensitive: true);

        /// <summary>
        /// Trims some unnecessary text from the end of this string, if it exists.
        /// </summary>
        /// <param name="unnecessaryText">Specific number of characters from this value that you want to be removed.</param>
        /// <param name="caseSensitive">By default it's TRUE.</param>
        public static string TrimEnd(this string @this, string unnecessaryText, bool caseSensitive)
        {
            if (unnecessaryText.IsEmpty() || @this.IsEmpty())
                return @this.OrEmpty();

            else if (@this.EndsWith(unnecessaryText, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
                return @this.TrimEnd(unnecessaryText.Length);

            else
                return @this;
        }

        /// <summary>
        /// Returns the last few characters of the string with a length
        /// specified by the given parameter. If the string's length is less than the 
        /// given length the complete string is returned. If length is zero or 
        /// less an empty string is returned
        /// </summary>
        /// <param name="length">Number of characters to return</param>
        public static string Right(this string @this, int length)
        {
            length = Math.Max(length, 0);

            if (@this.Length > length)
                return @this.Substring(@this.Length - length, length);
            else
                return @this;
        }

        /// <summary>
        /// Returns the first few characters of the string with a length
        /// specified by the given parameter. If the string's length is less than the 
        /// given length the complete string is returned. If length is zero or 
        /// less an empty string is returned
        /// </summary>
        /// <param name="length">Number of characters to return</param>
        public static string Left(this string @this, int length)
        {
            length = Math.Max(length, 0);

            if (@this.Length > length)
                return @this.Substring(0, length);
            else
                return @this;
        }

        /// <summary>
        /// This method identifies a string literal as an interpolated string.
        /// </summary>
        /// <param name="arg">The value which is used in this string.</param>
        /// <param name="additionalArgs">The list of values which are used in this string.</param>
        public static string FormatWith(this string @this, object arg, params object[] additionalArgs)
        {
            try
            {
                if (additionalArgs == null || additionalArgs.Length == 0)
                    return string.Format(@this, arg);
                else
                    return string.Format(@this, new object[] { arg }.Concat(additionalArgs).ToArray());
            }
            catch (Exception ex)
            {
                throw new FormatException("Cannot format the string '{0}' with the specified arguments.".FormatWith(@this), ex);
            }
        }

        /// <summary>
        /// Gets the last Char of a string.
        /// </summary>
        public static string GetLastChar(this string @this)
        {
            if (@this.HasValue())
            {
                if (@this.Length >= 1)
                    return @this.Substring(@this.Length - 1, 1);
                else
                    return @this;
            }
            else
                return null;
        }

        /// <summary>
        /// Gets whether this string item begins with any of the specified items{Args}.
        /// </summary>
        /// <param name="listOfBeginnings">The list of strings which are checked whether they are in this value or not.</param>
        public static bool StartsWithAny(this string @this, params string[] listOfBeginnings)
        {
            foreach (var option in listOfBeginnings)
                if (@this.StartsWith(option)) return true;

            return false;
        }

        /// <summary>
        /// Gets whether this string item begins with any of the specified items{Args}.
        /// </summary>
        /// <param name="listOfBeginnings">The list of strings which are checked whether they are in this value or not.</param>
        /// <param name="caseSensitive">The list of strings which are checked whether it is in this value or not.</param>
        public static bool StartsWithAny(this string @this, bool caseSensitive, params string[] listOfBeginnings)
        {
            foreach (var option in listOfBeginnings)
            {
                if (caseSensitive)
                {
                    if (@this.StartsWith(option)) return true;
                }
                else
                {
                    if (@this.StartsWith(option, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets whether this string item begins with any of the specified items{Args}.
        /// </summary>
        /// <param name="other">The string which is checked whether it is in this value.</param>
        /// <param name="caseSensitive">The list of strings which are checked whether it is in this value or not.</param>
        public static bool StartsWith(this string @this, string other, bool caseSensitive)
        {
            if (other.IsEmpty()) return false;

            if (caseSensitive) return @this.StartsWith(other);
            else return @this.StartsWith(other, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets whether this string item ends with any of the specified items.
        /// </summary>
        /// <param name="listOfEndings">The list of strings which are checked whether they are in this value or not.</param>
        public static bool EndsWithAny(this string @this, params string[] listOfEndings)
        {
            foreach (var option in listOfEndings)
                if (@this.EndsWith(option)) return true;

            return false;
        }

        /// <summary>
        /// Removes all Html tags from this html string.
        /// </summary>
        public static string RemoveHtmlTags(this string @this)
        {
            if (@this.IsEmpty()) return string.Empty;

            @this = @this
                .Replace("<br/>", Environment.NewLine)
                .Replace("<br />", Environment.NewLine)
                .Replace("<br>", Environment.NewLine)
                .Replace("<br >", Environment.NewLine)
                .Replace("<p>", Environment.NewLine);

            var from = new string[] {
                "&quot;", "&apos;", "&amp;", "&lt;", "&gt;", "&nbsp;",
                "&iexcl;","&cent;","&pound;","&curren;","&yen;","&brvbar;","&sect;","&uml;",
                "&copy;","&ordf;","&laquo;","&not;","&shy;","&reg;","&macr;","&deg;","&plusmn;",
                "&sup2;","&sup3;","&acute;","&micro;","&para;","&middot;","&cedil;","&sup1;",
                "&ordm;","&raquo;","&frac14;","&frac12;","&frac34;","&iquest;","&times;",
                "&divide;","&Agrave;","&Aacute;","&Acirc;","&Atilde;","&Auml;","&Aring;",
                "&AElig;","&Ccedil;","&Egrave;","&Eacute;","&Ecirc;","&Euml;","&Igrave;",
                "&Iacute;","&Icirc;","&Iuml;","&ETH;","&Ntilde;","&Ograve;","&Oacute;",
                "&Ocirc;","&Otilde;","&Ouml;","&Oslash;","&Ugrave;","&Uacute;","&Ucirc;",
                "&Uuml;","&Yacute;","&THORN;","&szlig;","&agrave;","&aacute;","&acirc;",
                "&atilde;","&auml;","&aring;","&aelig;","&ccedil;","&egrave;","&eacute;",
                "&ecirc;","&euml;","&igrave;","&iacute;","&icirc;","&iuml;","&eth;","&ntilde;",
                "&ograve;","&oacute;","&ocirc;","&otilde;","&ouml;","&oslash;","&ugrave;",
                "&uacute;","&ucirc;","&uuml;","&yacute;","&thorn;","&yuml;"};

            var to = new string[] { "\"", "'", "&", "<", ">", " ",
                "¡","¢","£","¤","¥","¦","§","¨","©","ª","«","¬","-","®","¯","°","±","²",
                "³","´","µ","¶","•","¸","¹","º","»","¼","½","¾","¿","×","÷","À","Á","Â",
                "Ã","Ä","Å","Æ","Ç","È","É","Ê","Ë","Ì","Í","Î","Ï","Ð","Ñ","Ò","Ó","Ô",
                "Õ","Ö","Ø","Ù","Ú","Û","Ü","Ý","Þ","ß","à","á","â","ã","ä","å","æ","ç",
                "è","é","ê","ë","ì","í","î","ï","ð","ñ","ò","ó","ô","õ","ö","ø","ù","ú",
                "û","ü","ý","þ","ÿ",
            };

            for (var i = 0; i < from.Length; i++)
                @this = @this.Replace(from[i], to[i]);

            return Regex.Replace(@this, @"<(.|\n)*?>", " ").Trim();
        }

        /// <summary>
        /// Gets all indices of a specified string inside this text.
        /// </summary>
        /// <param name="pattern">Finds {pattern} into this string and returns Inumerable value.</param>
        public static IEnumerable<int> AllIndicesOf(this string @this, string pattern)
        {
            if (pattern.IsEmpty())
                throw new ArgumentNullException(nameof(pattern));

            var result = new List<int>();

            var index = -1;

            do
            {
                index = @this.IndexOf(pattern, index + 1);
                if (index > -1) result.Add(index);
            }
            while (index > -1);

            return result;
        }

        /// <summary>
        /// Returns this text with the specified prefix if this has a value. If this text is empty or null, it will return empty string.
        /// </summary>
        public static string WithPrefix(this string @this, string prefix)
        {
            if (@this.IsEmpty()) return string.Empty;
            else return prefix + @this;
        }

        /// <summary>
        /// Returns this text with the specified suffix if this has a value. If this text is empty or null, it will return empty string.
        /// </summary>
        /// <param name="suffix">String which is inserted in the start of this value.</param>
        public static string WithSuffix(this string @this, string suffix)
        {
            if (@this.IsEmpty()) return string.Empty;
            else return @this + suffix;
        }

        /// <summary>
        /// Wraps this text between the left and right wrappers, only if this has a value.
        /// </summary>
        /// <param name="left">String which is located left side of this value.</param>
        /// <param name="right">String which is located right side of this value.</param>
        public static string WithWrappers(this string @this, string left, string right)
        {
            if (@this.IsEmpty()) return string.Empty;

            return left + @this + right;
        }

        /// <summary>
        /// Repeats this text by the number of times specified.
        /// </summary>
        /// <param name="times">The number of times that this value should be repeated.</param>
        public static string Repeat(this string @this, int times) => Repeat(@this, times, null);

        /// <summary>
        /// Repeats this text by the number of times specified separated with the specified separator.
        /// </summary>
        /// <param name="times">String which is located left side of this value.</param>
        /// <param name="separator">String which is located among all characters of this value.</param>
        public static string Repeat(this string @this, int times, string separator)
        {
            if (times < 0) throw new ArgumentOutOfRangeException(nameof(times), $"{nameof(times)} should be 0 or more.");

            if (times == 0) return string.Empty;

            var r = new StringBuilder();

            for (var i = 1; i <= times; i++)
            {
                r.Append(@this);

                if (!(separator is null)) r.Append(separator);
            }

            return r.ToString();
        }

        /// <summary>
        /// Determines if this string value contains a specified substring.
        /// </summary>
        /// <param name="subString">String which is checked.</param>
        /// <param name="caseSensitive">Determined whether case sensitive is important or not.</param>
        public static bool Contains(this string @this, string subString, bool caseSensitive)
        {
            if (@this is null && subString is null)
                return true;

            if (@this is null) return false;

            if (subString.IsEmpty()) return true;

            if (caseSensitive)
                return @this.Contains(subString);
            else
                return @this.ToUpper().Contains(subString?.ToUpper());
        }

        /// <summary>
        /// Removes the specified substrings from this string object.
        /// </summary>
        /// <param name="firstSubstringsToRemove">String which is removed.</param>
        /// <param name="otherSubstringsToRemove">A list of Strings which are removed.</param>
        public static string Remove(this string @this, string firstSubstringsToRemove, params string[] otherSubstringsToRemove) =>
            @this.Remove(firstSubstringsToRemove).Remove(otherSubstringsToRemove);

        /// <summary>
        /// Removes the specified substrings from this string object.
        /// </summary>
        /// <param name="firstSubstringsToRemove">String which is removed.</param>
        /// <param name="otherSubstringsToRemove">A list of Strings which are removed.</param>
        /// <param name="caseSensitive">Determined whether case sensitive is important or not.</param>
        public static string Remove(this string @this, string firstSubstringsToRemove, bool caseSensitive, params string[] otherSubstringsToRemove) =>
                @this.Remove(firstSubstringsToRemove, caseSensitive).Remove(otherSubstringsToRemove, caseSensitive);

        /// <summary>
        /// Removes the specified substrings from this string object.
        /// </summary>
        /// <param name="substringsToRemove">A list of Strings which are removed.</param>
        [EscapeGCop("It is the Except definition and so it cannot call itself")]
        public static string Remove(this string text, string[] substringsToRemove)
        {
            if (text.IsEmpty()) return text;

            var result = text;

            foreach (var sub in substringsToRemove)
                if (sub.HasValue())
                    result = result.Replace(sub, string.Empty);

            return result;
        }

        /// <summary>
        /// Removes the specified substrings from this string object.
        /// </summary>
        /// <param name="substringsToRemove">A list of Strings which are removed.</param>
        /// <param name="caseSensitive">Determines whether case sensitive is important or not.</param>
        public static string Remove(this string text, string[] substringsToRemove, bool caseSensitive)
        {
            if (text.IsEmpty()) return text;

            var result = text;
            var comparison = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;

            foreach (var sub in substringsToRemove)
                if (sub.HasValue())
                    result = Regex.Replace(result, sub, string.Empty, comparison);

            return result;
        }

        /// <summary>
        /// Removes the specified substrings from this string object.
        /// </summary>
        /// <param name="substringToRemove">String which is removed.</param>
        [EscapeGCop("It is the Except definition and so it cannot call itself")]
        public static string Remove(this string text, string substringToRemove)
        {
            if (text.IsEmpty()) return text;

            return text.Replace(substringToRemove, string.Empty);
        }

        /// <summary>
        /// Removes the specified substrings from this string object.
        /// </summary>
        /// <param name="substringToRemove">String which is removed.</param>
        /// <param name="caseSensitive">Determines whether case sensitive is important or not.</param>
        public static string Remove(this string text, string substringToRemove, bool caseSensitive)
        {
            if (text.IsEmpty()) return text;

            var comparison = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            return Regex.Replace(text, substringToRemove, string.Empty, comparison);
        }

        /// <summary>
        /// Replaces all occurrences of a specified phrase to a substitute, even if the original phrase gets produced again as the result of substitution. Note: It's an expensive call.
        /// </summary>
        /// <param name="original">String which is removed.</param>
        /// <param name="substitute">String which is replaced.</param>
        public static string KeepReplacing(this string @this, string original, string substitute)
        {
            if (@this.IsEmpty()) return @this;

            if (original == substitute) return @this; // prevent loop

            while (@this.Contains(original))
                @this = @this.Replace(original, substitute);

            return @this;
        }

        /// <summary>
        /// Replaces all occurrences of a specified phrase to a substitute, even if the original phrase gets produced again as the result of substitution. Note: It's an expensive call.
        /// </summary>
        /// <param name="original">String which is removed.</param>
        /// <param name="substitute">String which is replaced.</param>
        /// <param name="caseSensitive">Determines whether case sensitive is important or not. Default value is True.</param>
        public static string KeepReplacing(this string @this, string original, string substitute, bool caseSensitive)
        {
            if (@this.IsEmpty()) return @this;

            if (original == substitute) return @this; // prevent loop

            var comparison = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            @this = Regex.Replace(@this, original, substitute, comparison);

            return @this;
        }

        /// <summary>
        /// Gets this same string when a specified condition is True, otherwise it returns empty string.
        /// </summary>
        /// <param name="condition">The condition which is checked.</param>
        public static string OnlyWhen(this string @this, bool condition)
        {
            if (condition) return @this;
            else return string.Empty;
        }

        /// <summary>
        /// Gets this same string when a specified condition is False, otherwise it returns empty string.
        /// </summary>
        /// <param name="condition">The condition which is checked.</param>
        public static string Unless(this string @this, bool condition)
        {
            if (condition) return string.Empty;
            else return @this;
        }

        /// <summary>
        /// Gets the lines of this string.
        /// </summary>
        public static string[] ToLines(this string @this)
        {
            if (@this is null) return new string[0];

            return @this.Split('\n').Select(l => l.Trim('\r')).ToArray();
        }

        /// <summary>
        /// Indicates whether this character is categorized as an uppercase letter.
        /// </summary>
        public static bool IsUpper(this char @this) => char.IsUpper(@this);

        /// <summary>
        /// Indicates whether this character is categorized as a lowercase letter.
        /// </summary>
        public static bool IsLower(this char @this) => char.IsLower(@this);

        /// <summary>
        /// Indicates whether this character is categorized as a letter.
        /// </summary>
        public static bool IsLetter(this char @this) => char.IsLetter(@this);

        /// <summary>
        /// Determines this value is one of the {Args}.
        /// </summary>
        /// <param name="characters">The list of characters which are checked.</param>
        public static bool IsAnyOf(this char @this, params char[] characters) => characters.Contains(@this);

        /// <summary>
        /// Indicates whether this character is categorized as digit.
        /// </summary>
        public static bool IsDigit(this char @this) => char.IsDigit(@this);

        /// <summary>
        /// Indicates whether this character is categorized as White Space (space, tab, new line, etc.).
        /// </summary>
        public static bool IsWhiteSpace(this char @this) => char.IsWhiteSpace(@this);

        /// <summary>
        /// Indicates whether this character is categorized as a letter or digit.
        /// </summary>
        public static bool IsLetterOrDigit(this char @this) => char.IsLetterOrDigit(@this);

        /// <summary>
        /// Converts the value of this character to its uppercase equivalent.
        /// </summary>
        public static char ToUpper(this char @this) => char.ToUpper(@this);

        /// <summary>
        /// Converts the value of this character to its lowercase equivalent.
        /// </summary>
        public static char ToLower(this char @this) => char.ToLower(@this);

        /// <summary>
        /// If this expression is null, returns an empty string. Otherwise, it returns the ToString() of this instance.
        /// </summary>
        public static string ToStringOrEmpty(this object @this)
        {
            if (@this == null) return string.Empty;
            else return @this.ToString().Or(string.Empty);
        }

        /// <summary>
        /// Determines whether this string object does not contain the specified phrase.
        /// </summary>
        /// <param name="phrase">The string which is searched in this value.</param>
        /// <param name="caseSensitive">Determined whether case sensitive is important or not.</param>
        public static bool Lacks(this string @this, string phrase, bool caseSensitive = false)
        {
            if (@this.IsEmpty()) return phrase.HasValue();

            return !@this.Contains(phrase, caseSensitive);
        }

        /// <summary>
        /// Determines whether this string object does not contain any of the specified phrases.
        /// </summary>
        /// <param name="phrases">The list of strings which are searched in this value.</param>
        public static bool LacksAll(this string @this, params string[] phrases) =>
            LacksAll(@this, caseSensitive: false, phrases: phrases);

        /// <summary>
        /// Determines whether this string object does not contain any of the specified phrases.
        /// </summary>
        /// <param name="caseSensitive">Determined whether case sensitive is important or not.</param>
        /// <param name="phrases">The list of strings which are searched in this value.</param>
        public static bool LacksAll(this string @this, bool caseSensitive, params string[] phrases)
        {
            if (@this.IsEmpty()) return true;

            return phrases.None(p => p.HasValue() && @this.Contains(p, caseSensitive));
        }

        /// <summary>
        /// Returns natural English literal text for a specified pascal case string value.
        /// For example it coverts "ThisIsSomething" to "This is something".
        /// </summary>
        public static string ToLiteralFromPascalCase(this string @this)
        {
            if (@this.IsEmpty()) return string.Empty;

            return LiteralFromPascalCaseCache.GetOrAdd(@this, source =>
            {
                var parts = new List<string>();
                var lastPart = "";

                foreach (var c in source)
                {
                    if (c.IsUpper() && lastPart.HasValue())
                    {
                        parts.Add(lastPart);
                        lastPart = "";
                    }

                    lastPart += c;
                }

                parts.Add(lastPart);

                var result = parts.First() + parts.Skip(1).Select(a => a.Length < 2 ? a : " " + a.ToLower()).ToString("");
                return result.Trim();
            });
        }

        /// <summary>
        /// Returns the all-lower-case version of this list.
        /// </summary>
        public static IEnumerable<string> ToLower(this IEnumerable<string> @this) => @this.ExceptNull().Select(i => i.ToLower());

        /// <summary>
        /// Returns the all-upper-case version of this list.
        /// </summary>
        public static IEnumerable<string> ToUpper(this IEnumerable<string> @this) => @this.ExceptNull().Select(i => i.ToUpper());

        /// <summary>
        /// Gets the UTF8-with-signature bytes of this text.
        /// </summary>
        public static byte[] GetUtf8WithSignatureBytes(this string @this)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(@this);

            // Add signature:
            var result = new byte[bytes.Length + 3];

            // Utf-8 signature code: BOM
            result[0] = UTF8SignatureFirstByte;
            result[1] = UTF8SignatureSecondByte;
            result[2] = UTF8SignatureThirdByte;

            bytes.CopyTo(result, 3);

            return result;
        }

        /// <summary>
        /// Converts this array of bytes to a Base64 string.
        /// </summary>
        public static string ToBase64String(this byte[] @this)
        {
            if (@this == null) return null;

            return Convert.ToBase64String(@this);
        }

        /// <summary>
        /// Converts this Base64 string to an array of bytes.
        /// </summary>
        public static byte[] ToBytesFromBase64(this string @this)
        {
            if (@this.IsEmpty()) return new byte[0];

            return Convert.FromBase64String(@this);
        }

        /// <summary>
        /// Converts this string to an array of bytes with the given encoding.
        /// </summary>
        /// <param name="encoding">The Encoding which is used for this value.</param>
        public static byte[] ToBytes(this string @this, Encoding encoding) => encoding.GetBytes(@this);

        /// <summary>
        /// Determines whether this text contains any of the specified keywords.
        /// If the keywords list contains a null or empty string, it throws an exception. If you wish to ignore those, use .Trim() on your keywords list.
        /// </summary>
        /// <param name="keywords">The list of string which are checked.</param>
        /// <param name="caseSensitive">ِDetermines whether the case sensitive is important or not.</param>
        public static bool ContainsAny(this string @this, IEnumerable<string> keywords, bool caseSensitive = true)
        {
            if (keywords == null)
                throw new ArgumentNullException(nameof(keywords));

            if (@this.IsEmpty()) return false;

            foreach (var key in keywords)
            {
                if (key.IsEmpty())
                    throw new ArgumentException($"{nameof(keywords)} contains a null or empty string element.");

                if (@this.Contains(key, caseSensitive))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Splits this list of string items by a specified separator into a number of smaller lists of string.
        /// </summary>
        /// <param name="separator">ِDetermines separator string.</param>
        public static IEnumerable<List<string>> Split(this IEnumerable<string> @this, string separator)
        {
            var currentArray = new List<string>();

            foreach (var item in @this)
            {
                if (item == separator)
                {
                    if (currentArray.Any())
                    {
                        yield return currentArray;
                        currentArray = new List<string>();
                    }
                }
                else
                    currentArray.Add(item);
            }
        }

        /// <summary>
        /// Converts this path into a file object.
        /// </summary>
        [EscapeGCop("I AM the solution myself!")]
        public static FileInfo AsFile(this string @this)
        {
            if (@this.IsEmpty()) return null;
            return new FileInfo(@this);
        }

        /// <summary>
        /// It will search in all environment PATH directories, as well as the current directory, to find this file.
        /// For example for 'git.exe' it will return `C:\Program Files\Git\bin\git.exe`.
        /// </summary>
        public static FileInfo AsFile(this string @this, bool searchEnvironmentPath)
        {
            if (!searchEnvironmentPath) return @this.AsFile();

            var result = Environment.ExpandEnvironmentVariables(@this).AsFile();
            if (result.Exists()) return result;

            if (Path.GetDirectoryName(@this).IsEmpty())
            {
                var environmentFolders = Environment.GetEnvironmentVariable("PATH").OrEmpty().Split(';').Trim();
                foreach (var test in environmentFolders)
                {
                    result = test.AsDirectory().GetFile(@this);
                    if (result.Exists()) return result;
                }
            }

            throw new FileNotFoundException(new FileNotFoundException().Message, @this);
        }

        /// <summary>
        /// Converts this path into a Uri object.
        /// </summary>
        public static Uri AsUri(this string @this) => new Uri(@this);

        /// <summary>
        /// Converts this path into a directory object.
        /// </summary>
        public static System.IO.DirectoryInfo AsDirectory(this string @this)
        {
            if (@this.IsEmpty()) return null;
            return new System.IO.DirectoryInfo(@this);
        }

        /// <summary>
        /// Gets the Xml Encoded version of this text.
        /// </summary>
        public static string XmlEncode(this string @this)
        {
            if (@this.IsEmpty()) return string.Empty;

            foreach (var set in XMLEscapingChars)
                @this = @this.Replace(set[0], set[1]);

            return @this;
        }

        /// <summary>
        /// Gets the Xml Decoded version of this text.
        /// </summary>
        public static string XmlDecode(this string @this)
        {
            if (@this.IsEmpty()) return string.Empty;

            foreach (var set in XMLEscapingChars)
                @this = @this.Replace(set[1], set[0]);

            return @this;
        }

        /// <summary>
        /// Creates a hash of a specified clear text with a mix of MD5 and SHA1.
        /// </summary>
        /// <param name="salt">Is random data that is used as an additional input to a one-way function that "hashes" data, a password or passphrase.</param>
        public static string CreateHash(this string @this, object salt = null)
        {
            var firstHash = @this.CreateMD5Hash();

            firstHash = $"«6\"£k&36 2{firstHash}mmñÃ5d*";

            firstHash += salt.ToStringOrEmpty();

            return firstHash.CreateSHA1Hash();
        }

        /// <summary>
        /// Creates MD5 hash of this text
        /// <param name="asHex">Specifies whether a hex-compatible string is expected.</param>
        /// </summary>
        /// <param name="asHex">Determines whether MD5 is based on Hex or not.</param>
        public static string CreateMD5Hash(this string @this, bool asHex = false)
        {
            var value = MD5.Create().ComputeHash(UnicodeEncoding.UTF8.GetBytes(@this));

            if (asHex)
                return BitConverter.ToString(value).Remove("-");
            else
                return Convert.ToBase64String(value);
        }

        /// <summary>
        /// Creates MD5 hash of this text.
        /// </summary>
        public static string CreateMD5Hash(this string @this) =>
            Convert.ToBase64String(MD5.Create().ComputeHash(UnicodeEncoding.UTF8.GetBytes(@this)));

        /// <summary>
        /// Creates SHA1 hash of this text
        /// </summary>
        public static string CreateSHA1Hash(this string @this)
        {
            return Convert.ToBase64String(SHA1.Create().ComputeHash(UnicodeEncoding.UTF8.GetBytes(@this))).TrimEnd('=');
        }

        /// <summary>
        /// Creates SHA256 hash of this text
        /// </summary>
        public static string CreateSHA256Hash(this string @this)
        {
            using (var hash = SHA256Managed.Create())
            {
                return string.Concat(
                    hash
                    .ComputeHash(Encoding.UTF8.GetBytes(@this))
                    .Select(item => item.ToString("x2").ToLower())
                );
            }
        }

        /// <summary>
        /// Creates SHA512 hash of this text
        /// </summary>
        public static string CreateSHA512Hash(this string @this)
        {
            using (var hash = SHA512Managed.Create())
            {
                return string.Concat(
                    hash
                    .ComputeHash(Encoding.UTF8.GetBytes(@this))
                    .Select(item => item.ToString("x2").ToLower())
                );
            }
        }

        /// <summary>
        /// Splits this string into some IEnumerable strings which have {chunkSize} characters.
        /// </summary>
        /// <param name="chunkSize"> The size of chunks. If {chunkSize} is 1, it returns all this string.</param>
        public static IEnumerable<string> Split(this string @this, int chunkSize)
        {
            if (@this.HasValue())
            {
                if (@this.Length > chunkSize)
                {
                    yield return @this.Substring(0, chunkSize);
                    foreach (var part in @this.Substring(chunkSize).Split(chunkSize))
                        yield return part;
                }
                else yield return @this;
            }
        }

        /// <summary>
        /// Gets a piece of this string from specific start to specific end place..
        /// </summary>
        /// <param name="fromIndex"> Is an Integer argument that determines substring is started from which character index. The first character of this string is started from Zero.</param>
        /// <param name="toText"> Is a string that determines the end of the output string.</param>
        public static string Substring(this string @this, int fromIndex, string toText)
        {
            var toIndex = @this.IndexOf(toText, fromIndex + 1);

            if (fromIndex == -1) return string.Empty;

            if (toIndex == -1) return string.Empty;

            if (toIndex < fromIndex) return string.Empty;

            return @this.Substring(fromIndex, toIndex - fromIndex);
        }

        /// <summary>
        /// Gets a piece of this string from specific start to specific end place..
        /// </summary>
        /// <param name="fromIndex"> Is an Integer argument that determines substring is started from which character index. The first character of this string is started from Zero.</param>
        /// <param name="toText"> Is a string that determines the end of the output string.</param>
        /// <param name="inclusive"> Determines whether the output string contains {from} and {to} strings or not. Default value is false.</param>
        public static string Substring(this string @this, string from, string to, bool inclusive) =>
            Substring(@this, from, to, inclusive, caseSensitive: true);

        /// <summary>
        /// Gets a piece of this string from specific start to specific end place..
        /// </summary>
        /// <param name="from"> Is an Integer argument that determines substring is started from which character index. The first character of this string is started from Zero.</param>
        /// <param name="to"> Is a string that determines the end of the output string.</param>
        /// <param name="inclusive"> Determines whether the output string contains {from} and {to} strings or not. Default value is false.</param>
        /// <param name="caseSensitive"> Determines whether the case sensitive is important or not.</param>
        public static string Substring(this string @this, string from, string to, bool inclusive, bool caseSensitive)
        {
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            var fromIndex = @this.IndexOf(from, comparison);
            if (fromIndex == -1) return string.Empty;
            if (@this.Length == fromIndex + from.Length) return string.Empty;

            var toIndex = @this.IndexOf(to, fromIndex + from.Length + 1, comparison);
            if (toIndex == -1) return string.Empty;

            if (toIndex < fromIndex) return string.Empty;

            if (inclusive) toIndex += to.Length;
            else fromIndex += from.Length;

            return @this.Substring(fromIndex, toIndex - fromIndex);
        }

        /// <summary>
        /// Gets a piece of this string from specific start to specific end place.
        /// </summary>
        /// <param name="encoding">The Encoding that is base of convert to string.</param>
        public static string ToString(this byte[] @this, Encoding encoding) => encoding.GetString(@this);

        public static string ToASCII(this string @this) => @this.ToBytes(Encoding.ASCII).ToString(Encoding.ASCII);

        /// <summary>
        /// Escapes all invalid characters of this string to it's usable as a valid json constant.
        /// </summary>
        public static string ToJsonText(this string @this)
        {
            if (@this.IsEmpty()) return string.Empty;

            return @this.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\"", "\\\"");
        }

        /// <summary>
        /// Gets a SHA1 hash of this text where all characters are alpha numeric.
        /// </summary>
        public static string ToSimplifiedSHA1Hash(this string @this) =>
            new string(@this.CreateSHA1Hash().ToCharArray().Where(c => c.IsLetterOrDigit()).ToArray());

        /// <summary>
        /// Attempts to Parse this String as the given Enum type.
        /// </summary>
        public static T? TryParseEnum<T>(this string @this, T? @default = null) where T : struct
        {
            if (Enum.TryParse(@this, ignoreCase: true, result: out T value)) return value;
            else return @default;
        }

        /// <summary>
        /// If it's null, it return empty string. Otherwise it returns this.
        /// </summary>
        [EscapeGCop("I AM the solution myself!")]
        public static string OrEmpty(this string @this) => @this ?? string.Empty;

        /// <summary>
        /// Returns the only matched string in the given text using this Regex pattern. 
        /// Returns null if more than one match found.
        /// </summary>
        /// <param name="text">The string that is controlled whether is matched to Regex or not.</param>
        public static string GetSingleMatchedValueOrDefault(this Regex @this, string text)
        {
            var matches = @this.Matches(text).Cast<Match>()
                .Except(m => !m.Success || string.IsNullOrWhiteSpace(m.Value))
                .ToList();
            return matches.IsSingle() ? matches[0].Value : null;
        }

        /// <summary>
        /// Returns true if this collection has more than one item.
        /// </summary>
        public static bool HasMany<T>(this IEnumerable<T> @this)
        {
            using (var en = @this.GetEnumerator())
                return en.MoveNext() && en.MoveNext();
        }

        /// <summary>
        /// Returns a string value that can be saved in xml.
        /// </summary>
        public static string XmlEscape(this string @this)
        {
            if (@this.IsEmpty()) return string.Empty;

            foreach (var set in XMLEscapingChars.Take(4))
                @this = @this.Replace(set[0], set[1]);

            return @this;
        }

        /// <summary>
        /// Returns a string value without any xml-escaped characters.
        /// </summary>
        public static string XmlUnescape(this string @this)
        {
            if (@this.IsEmpty()) return string.Empty;

            foreach (var set in XMLEscapingChars.Take(4))
                @this = @this.Replace(set[1], set[0]);

            return @this;
        }

        /// <summary>
        /// Returns valid JavaScript string content with reserved characters replaced by encoded literals.
        /// </summary>
        public static string JavascriptEncode(this string @this)
        {
            foreach (var ch in JsUnsafeCharacters)
            {
                var replace = new string(ch, 1);
                var encoded = string.Format("\\x{0:X}", Convert.ToInt32(ch));
                @this = @this.Replace(replace, encoded);
            }

            @this = @this.Replace(Environment.NewLine, "\\n");

            return @this;
        }

        /// <summary>
        /// Returns valid PascalCase JavaScript or C# string content.
        /// </summary>
        public static string ToPascalCaseId(this string @this)
        {
            if (@this.IsEmpty()) return @this;

            return new PascalCaseIdGenerator(@this).Build();
        }

        /// <summary>
        /// Returns valid camelCase javaScript or C# string content.
        /// </summary>
        public static string ToCamelCaseId(this string @this)
        {
            var result = ToPascalCaseId(@this);

            if (result.IsEmpty()) return string.Empty;

            if (result.Length == 1) return result.ToLower();
            else return char.ToLower(result[0]) + result.Substring(1);
        }

        /// <summary>
        /// Converts [hello world] to [Hello World].
        /// </summary>
        public static string CapitaliseFirstLetters(this string @this)
        {
            if (@this.IsEmpty()) return @this;

            return @this.Split(' ').Trim().Select(x => x.First().ToUpper() + x.Substring(1)).ToString(" ");
        }

        [Obsolete("Use either RemoveBefore() or RemoveBeforeAndIncluding().", error: true)]
        public static string TrimBefore(this string @this, string search, bool trimPhrase = false, bool caseSensitive = false)
        {
            return @this.RemoveStart(search, trimPhrase, caseSensitive);
        }

        /// <summary>
        /// Removes all text before and including the specified search phrase.
        /// If the phrase is not found, the original string will be returned.
        /// </summary>
        public static string RemoveBeforeAndIncluding(this string @this, string search, bool caseSensitive = false)
        {
            return @this.RemoveStart(search, trimPhrase: true, caseSensitive: caseSensitive);
        }

        /// <summary>
        /// Removes all text before the first occurance of a specified search phrase.
        /// If the phrase is not found, the original string will be returned.
        /// </summary>
        public static string RemoveBefore(this string @this, string phrase, bool caseSensitive = false)
        {
            return @this.RemoveStart(phrase, trimPhrase: false, caseSensitive: caseSensitive);
        }

        static string RemoveStart(this string @this, string search, bool trimPhrase = false, bool caseSensitive = false)
        {
            if (@this.IsEmpty()) return @this;

            int index;

            if (caseSensitive) index = @this.IndexOf(search);
            else
                index = @this.IndexOf(search, StringComparison.OrdinalIgnoreCase);

            if (index == -1) return @this;

            @this = @this.Substring(index);

            if (trimPhrase) @this = @this.TrimStart(search, caseSensitive);

            return @this;
        }

        /// <summary>
        /// Removes all leading occurrences of a set of characters specified in this string. 
        /// The string that remains after all occurrences of characters in this string are removed from the start of the current string.
        /// </summary>
        /// <param name="search">It is removed from this value.</param>
        /// <param name="caseSensitive">Determines whether the case sensitive is important or not.</param>
        public static string TrimStart(this string @this, string search, bool caseSensitive)
        {
            if (caseSensitive) return @this.TrimStart(search);

            if (@this.StartsWith(search, caseSensitive: false))
                return @this.Substring(search.Length);

            return @this;
        }

        /// <summary>
        /// Removes all characters from the first occurrence of the specified phrase. 
        /// If the phrase is not found, the original string will be returned.
        /// </summary>        
        public static string RemoveFrom(this string @this, string phrase, bool caseSensitive = false)
        {
            return @this.RemoveFromOrAfter(phrase, trimPhrase: true, caseSensitive: caseSensitive);
        }

        /// <summary>
        /// Removes all characters from after the first occurrence of the specified phrase.
        /// If the phrase is not found, the original string will be returned.
        /// </summary>        
        public static string RemoveFromAfter(this string @this, string phrase, bool caseSensitive = false)
        {
            return @this.RemoveFromOrAfter(phrase, trimPhrase: false, caseSensitive: caseSensitive);
        }

        static string RemoveFromOrAfter(this string @this, string phrase, bool trimPhrase, bool caseSensitive)
        {
            if (@this.IsEmpty()) return @this;

            int index;

            if (caseSensitive) index = @this.IndexOf(phrase);
            else index = @this.IndexOf(phrase, StringComparison.OrdinalIgnoreCase);

            if (index == -1) return @this;

            if (!trimPhrase) index += phrase.Length;

            return @this.Substring(0, index);
        }

        [Obsolete("Use either RemoveFromAfter() or RemoveFrom().", error: true)]
        public static string TrimAfter(this string @this, string phrase, bool trimPhrase = true, bool caseSensitive = false)
        {
            if (trimPhrase) return @this.RemoveFrom(phrase, caseSensitive);
            else return @this.RemoveFromAfter(phrase, caseSensitive);
        }

        /// <summary>
        /// Returns this string. But if it's String.Empty, it returns NULL.
        /// </summary>
        public static string OrNullIfEmpty(this string @this)
        {
            if (string.Equals(@this, string.Empty)) return null;

            return @this;
        }

        /// <summary>
        /// Capitalises the first letter and lower-cases the rest.
        /// </summary>
        public static string ToProperCase(this string @this)
        {
            if (@this.IsEmpty()) return @this;

            return @this.First().ToUpper() + @this.Substring(1).ToLower();
        }

        /// <summary>
        /// It will replace all occurrences of a specified WHOLE WORD and skip occurrences of the word with characters or digits attached to it.
        /// </summary>
        /// <param name="word">Determines the string which is removed.</param>
        /// <param name="replacement">Determines the string which is replaced.</param>
        /// <param name="caseSensitive">Determines whether the case sensitive is important or not.</param>
        public static string ReplaceWholeWord(this string @this, string word, string replacement, bool caseSensitive = true)
        {
            var pattern = "\\b" + Regex.Escape(word) + "\\b";
            if (caseSensitive) return Regex.Replace(@this, pattern, replacement);
            else return Regex.Replace(@this, pattern, replacement, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Returns if a specified WHOLE WORD is found in this text. It skips occurrences of the word with characters or digits attached to it.
        /// </summary>
        /// <param name="word">Determines the string which is searched.</param>
        /// <param name="caseSensitive">Determines whether the case sensitive is important or not.</param>
        public static bool ContainsWholeWord(this string @this, string word, bool caseSensitive = true)
        {
            if (@this.IsEmpty()) return false;

            var pattern = "\\b" + Regex.Escape(word) + "\\b";

            if (caseSensitive) return Regex.IsMatch(@this, pattern);
            else return Regex.IsMatch(@this, pattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Converts the value of a boolean object to its equivalent string representation for the specified custom text instead of the default "True" or "False".
        /// </summary>
        /// <param name="trueText">The string which is returned if this value is true.</param>
        /// <param name="falseText">The string which is returned if this value is false.</param>
        [EscapeGCop("It is an extension for boolean type")]
        public static string ToString(this bool value, string trueText, string falseText) =>
            ToString(value, trueText, falseText, nullText: null);

        /// <summary>
        /// Converts the value of a boolean object to its equivalent string representation for the specified custom text instead of the default "True" or "False".
        /// </summary>
        /// <param name="trueText">The string which is returned if this value is true.</param>
        /// <param name="falseText">The string which is returned if this value is false.</param>
        /// <param name="nullText ">The string which is returned if this value is null.</param>
        public static string ToString(this bool? @this, string trueText, string falseText, string nullText = null)
        {
            if (@this == true) return trueText;
            else if (@this == false) return falseText;
            else return nullText;
        }

        /// <summary>
        /// Ensure that this string object starts with a specified other one.
        /// If it does not, then it prepends that and return the combined text.
        /// </summary>
        /// <param name="expression ">The string which is used for searching.</param>
        /// <param name="caseSensitive ">Determines which the case sensitive is important or not.</param>
        public static string EnsureStartsWith(this string @this, string expression, bool caseSensitive = true)
        {
            if (expression.IsEmpty()) return @this;

            if (@this.IsEmpty()) return expression;

            if (@this.StartsWith(expression, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) return @this;

            return expression + @this;
        }

        /// <summary>
        /// Ensure that this string object ends with a specified other one.
        /// If it does not, then it appends that and return the combined text.
        /// </summary>
        /// <param name="expression ">The string which is used for searching.</param>
        /// <param name="caseSensitive ">Determines which the case sensitive is important or not.</param>
        public static string EnsureEndsWith(this string @this, string expression, bool caseSensitive = true)
        {
            if (expression.IsEmpty()) return @this;

            if (@this.IsEmpty()) return expression;

            if (@this.EndsWith(expression, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) return @this;

            return @this + expression;
        }

        /// <summary>
        /// Gets the Html Encoded version of this text.
        /// </summary>
        public static string HtmlEncode(this string @this)
        {
            if (@this.IsEmpty()) return string.Empty;
            return System.Net.WebUtility.HtmlEncode(@this);
        }

        /// <summary>
        /// Gets the Html Decoded version of this text.
        /// </summary>
        public static string HtmlDecode(this string @this)
        {
            if (@this.IsEmpty()) return string.Empty;
            return System.Net.WebUtility.HtmlDecode(@this);
        }

        /// <summary>
        /// Gets the Url Encoded version of this text.
        /// </summary>
        public static string UrlEncode(this string @this)
        {
            if (@this.IsEmpty()) return string.Empty;
            return System.Net.WebUtility.UrlEncode(@this);
        }

        /// <summary>
        /// Gets the Url Decoded version of this text.
        /// </summary>
        public static string UrlDecode(this string @this)
        {
            if (@this.IsEmpty()) return string.Empty;
            return System.Net.WebUtility.UrlDecode(@this);
        }

        /// <summary>
        /// Removes unused characters from the content of a CSV file.
        /// </summary>
        public static string EscapeCsvValue(this string @this)
        {
            if (@this.IsEmpty()) return string.Empty;

            @this = @this.Remove("\r").Replace("\n", "\r\n");

            if (@this.Contains(",") || @this.Contains("\"") || @this.Contains("\n"))
                @this = "\"{0}\"".FormatWith(@this.Replace("\"", "\"\""));

            if (@this.StartsWithAny("+", "-", "@"))
                return "'" + @this;

            return @this;
        }
    }
}