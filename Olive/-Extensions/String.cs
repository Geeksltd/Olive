using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
        public static string TrimStart(this string text, string textToTrim)
        {
            if (text == null) text = string.Empty;

            if (textToTrim.IsEmpty() || text.IsEmpty()) return text;

            if (text.StartsWith(textToTrim)) return text.Substring(textToTrim.Length).TrimStart(textToTrim);
            else return text;
        }

        /// <summary>
        /// Trims the end of this instance of string with the specified number of characters.
        /// </summary>
        public static string TrimEnd(this string text, int numberOfCharacters)
        {
            if (numberOfCharacters < 0)
                throw new ArgumentException("numberOfCharacters must be greater than 0.");

            if (numberOfCharacters == 0) return text;

            if (text.IsEmpty() || text.Length <= numberOfCharacters)
                return string.Empty;

            return text.Substring(0, text.Length - numberOfCharacters);
        }

        /// <summary>
        /// If this string object is null, it will return null. Otherwise it will trim the text and return it.
        /// </summary>
        public static string TrimOrNull(this string text) => text?.Trim();

        /// <summary>
        /// If this string object is null, it will return empty string. Otherwise it will trim the text and return it.
        /// </summary>
        public static string TrimOrEmpty(this string text) => text.TrimOrNull().OrEmpty();

        public static bool IsNoneOf(this string text, params string[] items) => !text.IsAnyOf(items);

        /// <summary>
        /// Returns a copy of this text converted to lower case. If it is null it will return empty string.
        /// </summary>
        public static string ToLowerOrEmpty(this string text) => text.OrEmpty().ToLower();

        /// <summary>
        /// Returns a copy of this text converted to upper case. If it is null it will return empty string.
        /// </summary>
        public static string ToUpperOrEmpty(this string text) => text.OrEmpty().ToUpper();

        public static bool IsAnyOf(this string text, params string[] items)
        {
            if (text == null) return items.Any(x => x == null);

            return items.Contains(text);
        }

        public static bool IsAnyOf(this string text, IEnumerable<string> items) => IsAnyOf(text, items.ToArray());

        public static string EnsureStart(this string text, string startText, bool caseSensitive = false)
        {
            if (startText.IsEmpty())
                throw new ArgumentNullException(nameof(startText));

            if (text.IsEmpty()) return string.Empty;

            if (caseSensitive)
                if (text.StartsWith(startText))
                    return text;

                else
                {
                    if (text.ToLower().StartsWith(startText.ToLower()))
                        return text;
                }

            return startText + text;
        }

        public static bool ContainsAll(this string text, string[] keywords, bool caseSensitive)
        {
            if (!caseSensitive)
            {
                text = (text ?? string.Empty).ToLower();

                for (int i = 0; i < keywords.Length; i++) keywords[i] = keywords[i].ToLower();
            }

            foreach (var key in keywords)
                if (!text.Contains(key)) return false;

            return true;
        }

        /// <summary>
        /// Determines whether this instance of string is null or empty.
        /// </summary>

        public static bool IsEmpty(this string text) => string.IsNullOrEmpty(text);

        /// <summary>
        /// Determines whether this instance of string is not null or empty.
        /// </summary>

        public static bool HasValue(this string text) => !string.IsNullOrEmpty(text);

        /// <summary>
        /// Will replace all line breaks with a BR tag and return the result as a raw html.
        /// </summary>
        public static string ToHtmlLines(this string text) => text.OrEmpty().ToLines().ToString("<br/>");

        /// <summary>
        /// Will join all items with a BR tag and return the result as a raw html.
        /// </summary>
        public static string ToHtmlLines<T>(this IEnumerable<T> items) => items.ToString("<br/>");

        /// <summary>
        /// Gets the same string if it is not null or empty. Otherwise it returns the specified default value.
        /// </summary>
        public static string Or(this string text, string defaultValue)
        {
            if (string.IsNullOrEmpty(text)) return defaultValue;
            else return text;
        }

        /// <summary>
        /// Gets the same string if it is not null or empty.
        /// Otherwise it invokes the specified default value provider and returns the result.
        /// </summary>
        public static string Or(this string text, Func<string> defaultValueProvider)
        {
            if (string.IsNullOrEmpty(text)) return defaultValueProvider?.Invoke();
            else return text;
        }

        /// <summary>
        /// Gets the same string unless it is the same as the specified text. If they are the same, empty string will be returned.
        /// </summary>
        public static string Unless(this string text, string unwantedText)
        {
            if (text == unwantedText) return string.Empty;
            else return text;
        }

        /// <summary>
        /// Summarizes the specified source.
        /// </summary>
        public static string Summarize(this string text, int maximumLength, bool enforceMaxLength)
        {
            var result = Summarize(text, maximumLength);

            if (enforceMaxLength && result.Length > maximumLength)
                result = text.Substring(0, maximumLength - 3) + "...";

            return result;
        }

        /// <summary>
        /// Summarizes the specified text.
        /// </summary>        
        public static string Summarize(this string text, int maximumLength)
        {
            if (text.IsEmpty()) return text;

            if (text.Length > maximumLength)
            {
                text = text.Substring(0, maximumLength);

                var lastSpace = -1;

                foreach (char wordSeperator in " \r\n\t")
                    lastSpace = Math.Max(text.LastIndexOf(wordSeperator), lastSpace);

                if (lastSpace > maximumLength / 2)
                    text = text.Substring(0, lastSpace);

                text += "...";
            }

            return text;
        }

        #region Count string

        public static string Count<T>(this IEnumerable<T> list, string objectTitle)
        {
            if (objectTitle.IsEmpty())
                objectTitle = SeparateAtUpperCases(typeof(T).Name);

            return objectTitle.ToCountString(list.Count());
        }

        public static string Count<T>(this IEnumerable<T> list, string objectTitle, string zeroQualifier)
        {
            if (objectTitle.IsEmpty())
                objectTitle = SeparateAtUpperCases(typeof(T).Name);

            return objectTitle.ToCountString(list.Count(), zeroQualifier);
        }

        public static string ToCountString(this string name, int count)
        {
            var zeroQualifier = "no";

            if (name.HasValue() && char.IsUpper(name[0]))
                zeroQualifier = "No";

            return ToCountString(name, count, zeroQualifier);
        }

        public static string ToCountString(this string name, int count, string zeroQualifier)
        {
            name = name.Or("").Trim();
            if (name.IsEmpty())
                throw new Exception("'name' cannot be empty for ToCountString().");

            if (count < 0)
                throw new ArgumentException("count should be greater than or equal to 0.");

            if (count == 0) return zeroQualifier + " " + name;
            else if (count == 1) return "1 " + name;
            else return $"{count} {name.ToPlural()}";
        }

        public static string SeparateAtUpperCases(this string pascalCase)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < pascalCase.Length; i++)
            {
                if (char.IsUpper(pascalCase[i]) && i > 0)
                    sb.Append(" ");
                sb.Append(pascalCase[i]);
            }

            return sb.ToString().ToLower();
        }

        public static string ToPlural(this string singular)
        {
            if (singular.IsEmpty()) return string.Empty;

            // Only change the last word:
            var phrase = singular;
            var prefix = "";
            if (phrase.Split(' ').Length > 1)
            {
                // Multi word, set prefix to anything but the last word:
                prefix = phrase.Substring(0, phrase.LastIndexOf(" ")) + " ";
                singular = phrase.Substring(phrase.LastIndexOf(" ") + 1);
            }

            string plural;
            var irregular = GetIrregularPlural(singular);

            if (irregular != "")
            {
                if (prefix == "")
                    irregular = char.ToUpper(irregular[0]) + irregular.Substring(1);

                plural = irregular;
            }
            else plural = GetRegularPlural(singular);

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

            if (ending == 's' || (secondEnding.ToString() + ending) == "ch" || (secondEnding.ToString() + ending) == "sh")
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

                default: return "";
            }
        }

        #endregion

        /// <summary>
        /// Trims some unnecessary text from the end of this string, if it exists.
        /// </summary>
        public static string TrimEnd(this string text, string unnecessaryText) => TrimEnd(text, unnecessaryText, caseSensitive: true);

        /// <summary>
        /// Trims some unnecessary text from the end of this string, if it exists.
        /// </summary>
        /// <param name="caseSensitive">By default it's TRUE.</param>
        public static string TrimEnd(this string text, string unnecessaryText, bool caseSensitive)
        {
            if (unnecessaryText.IsEmpty() || text.IsEmpty())
                return text.OrEmpty();

            else if (text.EndsWith(unnecessaryText, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase))
                return text.TrimEnd(unnecessaryText.Length);

            else
                return text;
        }

        /// <summary>
        /// Returns the last few characters of the string with a length
        /// specified by the given parameter. If the string's length is less than the 
        /// given length the complete string is returned. If length is zero or 
        /// less an empty string is returned
        /// </summary>
        /// <param name="length">Number of characters to return</param>
        public static string Right(this string text, int length)
        {
            length = Math.Max(length, 0);

            if (text.Length > length)
                return text.Substring(text.Length - length, length);
            else
                return text;
        }

        /// <summary>
        /// Returns the first few characters of the string with a length
        /// specified by the given parameter. If the string's length is less than the 
        /// given length the complete string is returned. If length is zero or 
        /// less an empty string is returned
        /// </summary>
        /// <param name="length">Number of characters to return</param>
        public static string Left(this string text, int length)
        {
            length = Math.Max(length, 0);

            if (text.Length > length)
                return text.Substring(0, length);
            else
                return text;
        }

        public static string FormatWith(this string format, object arg, params object[] additionalArgs)
        {
            try
            {
                if (additionalArgs == null || additionalArgs.Length == 0)
                    return string.Format(format, arg);
                else
                    return string.Format(format, new object[] { arg }.Concat(additionalArgs).ToArray());
            }
            catch (Exception ex)
            {
                throw new FormatException("Cannot format the string '{0}' with the specified arguments.".FormatWith(format), ex);
            }
        }

        public static string GetLastChar(this string input)
        {
            if (input.HasValue())
            {
                if (input.Length >= 1)
                    return input.Substring(input.Length - 1, 1);
                else
                    return input;
            }
            else
                return null;
        }

        public static bool StartsWithAny(this string input, params string[] listOfBeginnings)
        {
            foreach (var option in listOfBeginnings)
                if (input.StartsWith(option)) return true;

            return false;
        }

        public static bool StartsWith(this string input, string other, bool caseSensitive)
        {
            if (other.IsEmpty()) return false;

            if (caseSensitive) return input.StartsWith(other);
            else return input.StartsWith(other, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets whether this string item ends with any of the specified items.
        /// </summary>
        public static bool EndsWithAny(this string input, params string[] listOfEndings)
        {
            foreach (var option in listOfEndings)
                if (input.EndsWith(option)) return true;

            return false;
        }

        /// <summary>
        /// Removes all Html tags from this html string.
        /// </summary>
        public static string RemoveHtmlTags(this string source)
        {
            if (source.IsEmpty()) return string.Empty;

            source = source
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

            for (int i = 0; i < from.Length; i++)
                source = source.Replace(from[i], to[i]);

            return Regex.Replace(source, @"<(.|\n)*?>", " ").Trim();
        }

        /// <summary>
        /// Gets all indices of a specified string inside this text.
        /// </summary>
        public static IEnumerable<int> AllIndicesOf(this string text, string pattern)
        {
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            var result = new List<int>();

            var index = -1;

            do
            {
                index = text.IndexOf(pattern, index + 1);
                if (index > -1)
                    result.Add(index);
            }
            while (index > -1);

            return result;
        }

        /// <summary>
        /// Returns this text with the specified prefix if this has a value. If this text is empty or null, it will return empty string.
        /// </summary>
        public static string WithPrefix(this string text, string prefix)
        {
            if (text.IsEmpty()) return string.Empty;
            else return prefix + text;
        }

        /// <summary>
        /// Returns this text with the specified suffix if this has a value. If this text is empty or null, it will return empty string.
        /// </summary>
        public static string WithSuffix(this string text, string suffix)
        {
            if (text.IsEmpty()) return string.Empty;
            else return text + suffix;
        }

        /// <summary>
        /// Wraps this text between the left and right wrappers, only if this has a value.
        /// </summary>
        public static string WithWrappers(this string text, string left, string right)
        {
            if (text.IsEmpty())
                return string.Empty;

            return left + text + right;
        }

        /// <summary>
        /// Repeats this text by the number of times specified.
        /// </summary>
        public static string Repeat(this string text, int times) => Repeat(text, times, null);

        /// <summary>
        /// Repeats this text by the number of times specified, seperated with the specified seperator.
        /// </summary>
        public static string Repeat(this string text, int times, string seperator)
        {
            if (times < 0) throw new ArgumentOutOfRangeException(nameof(times), $"{nameof(times)} should be 0 or more.");

            if (times == 0) return string.Empty;

            var r = new StringBuilder();

            for (var i = 1; i <= times; i++)
            {
                r.Append(text);

                if (seperator != null) r.Append(seperator);
            }

            return r.ToString();
        }

        /// <summary>
        /// Determines if this string value contains a specified substring.
        /// </summary>
        public static bool Contains(this string text, string subString, bool caseSensitive)
        {
            if (text == null && subString == null)
                return true;

            if (text == null) return false;

            if (subString.IsEmpty()) return true;

            if (caseSensitive)
                return text.Contains(subString);
            else
                return text.ToUpper().Contains(subString?.ToUpper());
        }

        /// <summary>
        /// Removes the specified substrings from this string object.
        /// </summary>
        public static string Remove(this string text, string firstSubstringsToRemove, params string[] otherSubstringsToRemove) =>
            text.Remove(firstSubstringsToRemove).Remove(otherSubstringsToRemove);

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
		[EscapeGCop("It is the Except definition and so it cannot call itself")]
        public static string Remove(this string text, string substringToRemove)
        {
            if (text.IsEmpty()) return text;

            return text.Replace(substringToRemove, string.Empty);
        }

        /// <summary>
        /// Replaces all occurances of a specified phrase to a substitude, even if the original phrase gets produced again as the result of substitution. Note: It's an expensive call.
        /// </summary>
        public static string KeepReplacing(this string text, string original, string substitute)
        {
            if (text.IsEmpty()) return text;

            if (original == substitute) return text; // prevent loop

            while (text.Contains(original))
                text = text.Replace(original, substitute);

            return text;
        }

        /// <summary>
        /// Gets this same string when a specified condition is True, otherwise it returns empty string.
        /// </summary>
        public static string OnlyWhen(this string text, bool condition)
        {
            if (condition)
                return text;
            else
                return string.Empty;
        }

        /// <summary>
        /// Gets this same string when a specified condition is False, otherwise it returns empty string.
        /// </summary>
        public static string Unless(this string text, bool condition)
        {
            if (condition)
                return string.Empty;
            else
                return text;
        }

        /// <summary>
        /// Gets the lines of this string.
        /// </summary>
        public static string[] ToLines(this string text)
        {
            if (text == null) return new string[0];

            return text.Split('\n').Select(l => l.Trim('\r')).ToArray();
        }

        /// <summary>
        /// Indicates whether this character is categorized as an uppercase letter.
        /// </summary>
        public static bool IsUpper(this char character) => char.IsUpper(character);

        /// <summary>
        /// Indicates whether this character is categorized as a lowercase letter.
        /// </summary>
        public static bool IsLower(this char character) => char.IsLower(character);

        /// <summary>
        /// Indicates whether this character is categorized as a letter.
        /// </summary>
        public static bool IsLetter(this char character) => char.IsLetter(character);

        public static bool IsAnyOf(this char character, params char[] characters) => characters.Contains(character);

        /// <summary>
        /// Indicates whether this character is categorized as digit.
        /// </summary>
        public static bool IsDigit(this char character) => char.IsDigit(character);

        /// <summary>
        /// Indicates whether this character is categorized as White Space (space, tab, new line, etc).
        /// </summary>
        public static bool IsWhiteSpace(this char character) => char.IsWhiteSpace(character);

        /// <summary>
        /// Indicates whether this character is categorized as a letter or digit.
        /// </summary>
        public static bool IsLetterOrDigit(this char character) => char.IsLetterOrDigit(character);

        /// <summary>
        /// Converts the value of this character to its uppercase equivalent.
        /// </summary>
        public static char ToUpper(this char character) => char.ToUpper(character);

        /// <summary>
        /// Converts the value of this character to its lowercase equivalent.
        /// </summary>
        public static char ToLower(this char character) => char.ToLower(character);

        /// <summary>
        /// If this expression is null, returns an empty string. Otherwise, it returns the ToString() of this instance.
        /// </summary>
        public static string ToStringOrEmpty(this object @object)
        {
            if (@object == null)
                return string.Empty;
            else
                return @object.ToString().Or(string.Empty);
        }

        /// <summary>
        /// Determines whether this string object does not contain the specified phrase.
        /// </summary>
        public static bool Lacks(this string text, string phrase, bool caseSensitive = false)
        {
            if (text.IsEmpty())
                return phrase.HasValue();

            return !text.Contains(phrase, caseSensitive);
        }

        /// <summary>
        /// Determines whether this string object does not contain any of the specified phrases.
        /// </summary>
        public static bool LacksAll(this string text, params string[] phrases) =>
            LacksAll(text, caseSensitive: false, phrases: phrases);

        /// <summary>
        /// Determines whether this string object does not contain any of the specified phrases.
        /// </summary>
        public static bool LacksAll(this string text, bool caseSensitive, params string[] phrases)
        {
            if (text.IsEmpty()) return true;

            return phrases.None(p => p.HasValue() && text.Contains(p, caseSensitive));
        }

        /// <summary>
        /// Returns natural English literal text for a specified pascal case string value.
        /// For example it coverts "ThisIsSomething" to "This is something".
        /// </summary>
        public static string ToLiteralFromPascalCase(this string pascalCaseText)
        {
            if (pascalCaseText.IsEmpty()) return string.Empty;

            return LiteralFromPascalCaseCache.GetOrAdd(pascalCaseText, source =>
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
        public static IEnumerable<string> ToLower(this IEnumerable<string> list) => list.ExceptNull().Select(i => i.ToLower());

        /// <summary>
        /// Returns the all-upper-case version of this list.
        /// </summary>
        public static IEnumerable<string> ToUpper(this IEnumerable<string> list) => list.ExceptNull().Select(i => i.ToUpper());

        /// <summary>
        /// Gets the UTF8-with-signature bytes of this text.
        /// </summary>
        public static byte[] GetUtf8WithSignatureBytes(this string text)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);

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
        public static string ToBase64String(this byte[] value)
        {
            if (value == null) return null;

            return Convert.ToBase64String(value);
        }

        /// <summary>
        /// Converts this Base64 string to an array of bytes.
        /// </summary>
        public static byte[] ToBytes(this string value)
        {
            if (value.IsEmpty()) return new byte[0];

            return Convert.FromBase64String(value);
        }

        /// <summary>
        /// Converts this string to an array of bytes with the given encoding.
        /// </summary>
        public static byte[] ToBytes(this string value, Encoding encoding) => encoding.GetBytes(value);

        /// <summary>
        /// Determines whether this text contains any of the specified keywords.
        /// If the keywords list contains a null or empty string, it throws an exception. If you wish to ignore those, use .Trim() on your keywords list.
        /// </summary>
        public static bool ContainsAny(this string text, IEnumerable<string> keywords, bool caseSensitive = true)
        {
            if (keywords == null)
                throw new ArgumentNullException(nameof(keywords));

            if (text.IsEmpty()) return false;

            foreach (var key in keywords)
            {
                if (key.IsEmpty()) throw new ArgumentException($"nameof(keywords) contains a null or empty string element.");

                if (text.Contains(key, caseSensitive))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Splits this list of string items by a specified separator into a number of smaller lists of string.
        /// </summary>
        public static IEnumerable<List<string>> Split(this IEnumerable<string> list, string separator)
        {
            var currentArray = new List<string>();

            foreach (var item in list)
            {
                if (item == separator)
                {
                    if (currentArray.Count > 0)
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
        public static System.IO.FileInfo AsFile(this string path) => new System.IO.FileInfo(path);

        /// <summary>
        /// Converts this path into a Uri object.
        /// </summary>
        public static Uri AsUri(this string path) => new Uri(path);

        /// <summary>
        /// Converts this path into a directory object.
        /// </summary>
        public static System.IO.DirectoryInfo AsDirectory(this string path) => new System.IO.DirectoryInfo(path);

        /// <summary>
        /// Gets the Xml Encoded version of this text.
        /// </summary>
        public static string XmlEncode(this string text)
        {
            if (text.IsEmpty()) return string.Empty;

            foreach (var set in XMLEscapingChars)
                text = text.Replace(set[0], set[1]);

            return text;
        }

        /// <summary>
        /// Gets the Xml Decoded version of this text.
        /// </summary>
        public static string XmlDecode(this string text)
        {
            if (text.IsEmpty()) return string.Empty;

            foreach (var set in XMLEscapingChars)
                text = text.Replace(set[1], set[0]);

            return text;
        }

        /// <summary>
        /// Creates a hash of a specified clear text with a mix of MD5 and SHA1.
        /// </summary>
        public static string CreateHash(this string clearText, object salt = null)
        {
            var firstHash = clearText.CreateMD5Hash();

            firstHash = $"«6\"£k&36 2{firstHash}mmñÃ5d*";

            firstHash += salt.ToStringOrEmpty();

            return firstHash.CreateSHA1Hash();
        }

        /// <summary>
        /// Creates MD5 hash of this text
        /// <param name="asHex">Specifies whether a hex-compatible string is expected.</param>
        /// </summary>
        public static string CreateMD5Hash(this string clearText, bool asHex = false)
        {
            var value = MD5.Create().ComputeHash(UnicodeEncoding.UTF8.GetBytes(clearText));

            if (asHex)
                return BitConverter.ToString(value).Remove("-");
            else
                return Convert.ToBase64String(value);
        }

        /// <summary>
        /// Creates MD5 hash of this text
        /// </summary>
        public static string CreateMD5Hash(this string clearText) =>
            Convert.ToBase64String(MD5.Create().ComputeHash(UnicodeEncoding.UTF8.GetBytes(clearText)));

        /// <summary>
        /// Creates SHA1 hash of this text
        /// </summary>
        public static string CreateSHA1Hash(this string clearText)
        {
            return Convert.ToBase64String(SHA1.Create().ComputeHash(UnicodeEncoding.UTF8.GetBytes(clearText))).TrimEnd('=');
        }
        /// <summary>
        /// Creates SHA256 hash of this text
        /// </summary>
        public static string CreateSHA256Hash(this string inputString)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                return String.Concat(
                    hash
                    .ComputeHash(Encoding.UTF8.GetBytes(inputString))
                    .Select(item => item.ToString("x2").ToLower())
                );
            }
        }
        /// <summary>
        /// Creates SHA512 hash of this text
        /// </summary>
        public static string CreateSHA512Hash(this string inputString)
        {
            using (SHA512 hash = SHA512Managed.Create())
            {
                return String.Concat(
                    hash
                    .ComputeHash(Encoding.UTF8.GetBytes(inputString))
                    .Select(item => item.ToString("x2").ToLower())
                );
            }
        }

        public static IEnumerable<string> Split(this string text, int chunkSize)
        {
            if (text.HasValue())
            {
                if (text.Length > chunkSize)
                {
                    yield return text.Substring(0, chunkSize);
                    foreach (var part in text.Substring(chunkSize).Split(chunkSize))
                        yield return part;
                }
                else yield return text;
            }
        }

        public static string Substring(this string text, int fromIndex, string toText)
        {
            var toIndex = text.IndexOf(toText, fromIndex + 1);

            if (fromIndex == -1) return string.Empty;

            if (toIndex == -1) return string.Empty;

            if (toIndex < fromIndex) return string.Empty;

            return text.Substring(fromIndex, toIndex - fromIndex);
        }

        public static string Substring(this string text, string from, string to, bool inclusive) =>
            Substring(text, from, to, inclusive, caseSensitive: true);

        public static string Substring(this string text, string from, string to, bool inclusive, bool caseSensitive)
        {
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            var fromIndex = text.IndexOf(from, comparison);
            var toIndex = text.IndexOf(to, fromIndex + from.Length + 1, comparison);

            if (fromIndex == -1)
                return string.Empty;

            if (toIndex == -1)
                return string.Empty;

            if (toIndex < fromIndex)
                return string.Empty;

            if (inclusive) toIndex += to.Length;
            else fromIndex += from.Length;

            return text.Substring(fromIndex, toIndex - fromIndex);
        }

        public static string ToString(this byte[] data, Encoding encoding) => encoding.GetString(data);

        /// <summary>
        /// Escapes all invalid characters of this string to it's usable as a valid json constant.
        /// </summary>
        public static string ToJsonText(this string source)
        {
            if (source.IsEmpty()) return string.Empty;

            return source.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\"", "\\\"");
        }

        /// <summary>
        /// Getsa SHA1 hash of this text where all characters are alpha numeric.
        /// </summary>
        public static string ToSimplifiedSHA1Hash(this string clearText) =>
            new string(clearText.CreateSHA1Hash().ToCharArray().Where(c => c.IsLetterOrDigit()).ToArray());

        /// <summary>
        /// Attempts to Parse this String as the given Enum type.
        /// </summary>
        public static T? TryParseEnum<T>(this string text, T? @default = null) where T : struct
        {
            if (Enum.TryParse(text, ignoreCase: true, result: out T value)) return value;

            return @default;
        }

        /// <summary>
        /// If it's null, it return empty string. Otherwise it returns this.
        /// </summary>
        public static string OrEmpty(this string text)
        {
            if (text == null) return string.Empty;
            return text;
        }

        /// <summary>
        /// Returns the only matched string in the given text using this Regex pattern. 
        /// Returns null if more than one match found.
        /// </summary>
        public static string GetSingleMatchedValueOrDefault(this Regex pattern, string text)
        {
            var matches = pattern.Matches(text).Cast<Match>()
                .Except(m => !m.Success || string.IsNullOrWhiteSpace(m.Value))
                .ToList();
            return matches.Count == 1 ? matches[0].Value : null;
        }

        /// <summary>
        /// Returns true if this collection has more than one item.
        /// </summary>
        public static bool HasMany<T>(this IEnumerable<T> collection)
        {
            using (var en = collection.GetEnumerator())
                return en.MoveNext() && en.MoveNext();
        }

        /// <summary>
        /// Returns a string value that can be saved in xml.
        /// </summary>
        public static string XmlEscape(this string unescaped)
        {
            if (unescaped.IsEmpty()) return string.Empty;

            foreach (var set in XMLEscapingChars.Take(4))
                unescaped = unescaped.Replace(set[0], set[1]);

            return unescaped;
        }

        /// <summary>
        /// Returns a string value without any xml-escaped characters.
        /// </summary>
        public static string XmlUnescape(this string escaped)
        {
            if (escaped.IsEmpty()) return string.Empty;

            foreach (var set in XMLEscapingChars.Take(4))
                escaped = escaped.Replace(set[1], set[0]);

            return escaped;
        }

        /// <summary>
        /// Returns valid JavaScript string content with reserved characters replaced by encoded literals.
        /// </summary>
        public static string JavascriptEncode(this string text)
        {
            foreach (var ch in JsUnsafeCharacters)
            {
                var replace = new string(ch, 1);
                var encoded = string.Format("\\x{0:X}", Convert.ToInt32(ch));
                text = text.Replace(replace, encoded);
            }

            text = text.Replace(Environment.NewLine, "\\n");

            return text;
        }

        /// <summary>
        /// Returns valid PascalCase JavaScript or C# string content.
        /// </summary>
        public static string ToPascalCaseId(this string text)
        {
            if (text.IsEmpty()) return text;

            return new PascalCaseIdGenerator(text).Build();
        }

        /// <summary>
        /// Returns valid camelCase javaScript or C# string content.
        /// </summary>
        public static string ToCamelCaseId(this string text)
        {
            var result = ToPascalCaseId(text);

            if (result.IsEmpty()) return string.Empty;

            if (result.Length == 1) return result.ToLower();
            else return char.ToLower(result[0]) + result.Substring(1);
        }

        /// <summary>
        /// Converts [hello world] to [Hello World].
        /// </summary>
        public static string CapitaliseFirstLetters(this string name)
        {
            if (name.IsEmpty()) return name;

            return name.Split(' ').Trim().Select(x => x.First().ToUpper() + x.Substring(1)).ToString(" ");
        }

        /// <summary>
        /// Trims all text before the specified search phrase.
        /// </summary>
        public static string TrimBefore(this string text, string search, bool caseSensitive = false, bool trimPhrase = false)
        {
            if (text.IsEmpty()) return text;

            int index;

            if (caseSensitive) index = text.IndexOf(search);
            else
                index = text.IndexOf(search, StringComparison.OrdinalIgnoreCase);

            if (index == -1) return text;

            text = text.Substring(index);

            if (trimPhrase) text = text.TrimStart(search, caseSensitive);

            return text;
        }

        public static string TrimStart(this string text, string search, bool caseSensitive)
        {
            if (caseSensitive) return text.TrimStart(search);

            if (text.StartsWith(search, caseSensitive: false))
                return text.Substring(search.Length);

            return text;
        }

        public static string TrimAfter(this string text, string phrase, bool trimPhrase = true, bool caseSensitive = false)
        {
            if (text.IsEmpty()) return text;

            int index;

            if (caseSensitive) index = text.IndexOf(phrase);
            else
                index = text.IndexOf(phrase, StringComparison.OrdinalIgnoreCase);

            if (index == -1) return text;

            if (!trimPhrase) index += phrase.Length;

            return text.Substring(0, index);
        }

        /// <summary>
        /// Returns this string. But if it's String.Empty, it returns NULL.
        /// </summary>
        public static string OrNullIfEmpty(this string text)
        {
            if (string.Equals(text, string.Empty)) return null;

            return text;
        }

        /// <summary>
        /// Capitalises the first letter and lower-cases the rest.
        /// </summary>
        public static string ToProperCase(this string name)
        {
            if (name.IsEmpty()) return name;

            return name.First().ToUpper() + name.Substring(1).ToLower();
        }

        /// <summary>
        /// It will replace all occurances of a specified WHOLE WORD and skip occurances of the word with characters or digits attached to it.
        /// </summary>
        public static string ReplaceWholeWord(this string text, string word, string replacement, bool caseSensitive = true)
        {
            var pattern = "\\b" + Regex.Escape(word) + "\\b";
            if (caseSensitive) return Regex.Replace(text, pattern, replacement);
            else return Regex.Replace(text, pattern, replacement, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Returns if a specified WHOLE WORD is found in this text. It skips occurances of the word with characters or digits attached to it.
        /// </summary>
        public static bool ContainsWholeWord(this string text, string word, bool caseSensitive = true)
        {
            if (text.IsEmpty()) return false;

            var pattern = "\\b" + Regex.Escape(word) + "\\b";

            if (caseSensitive) return Regex.IsMatch(text, pattern);
            else return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }

        [EscapeGCop("It is an extension for boolean type")]
        public static string ToString(this bool value, string trueText, string falseText) =>
            ToString(value, trueText, falseText, nullText: null);

        public static string ToString(this bool? value, string trueText, string falseText, string nullText = null)
        {
            if (value == true) return trueText;
            else if (value == false) return falseText;
            else return nullText;
        }

        /// <summary>
        /// Ensure that this string object starts with a specified other one.
        /// If it does not, then it prepends that and return the combined text.
        /// </summary>
        public static string EnsureStartsWith(this string text, string expression, bool caseSensitive = true)
        {
            if (expression.IsEmpty()) return text;

            if (text.IsEmpty()) return expression;

            if (text.StartsWith(expression, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) return text;

            return expression + text;
        }

        /// <summary>
        /// Ensure that this string object ends with a specified other one.
        /// If it does not, then it appends that and return the combined text.
        /// </summary>
        public static string EnsureEndsWith(this string text, string expression, bool caseSensitive = true)
        {
            if (expression.IsEmpty()) return text;

            if (text.IsEmpty()) return expression;

            if (text.EndsWith(expression, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) return text;

            return text + expression;
        }
    }
}