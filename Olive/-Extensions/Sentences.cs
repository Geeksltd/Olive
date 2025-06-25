namespace Olive
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    partial class OliveExtensions
    {
        static readonly HashSet<string> _abbreviations = new HashSet<string>(
        new[]
        {
            "Mr", "Mrs", "Ms", "Miss", "Dr", "Prof", "Sr", "Jr", "St", "Lt", "Col", "Gen", "Rev", "Fr", "Sgt",
            "Capt", "Maj", "Adm", "Ave", "Blvd", "Rd", "Ln", "Mt", "Ft",
            "e.g", "i.e", "etc", "vs", "Jan", "Feb", "Mar", "Apr", "Jun", "Jul", "Aug", "Sep", "Sept", "Oct", "Nov", "Dec",
            "a.m", "p.m", "U.S", "U.K", "Ph.D", "M.D", "B.Sc", "M.Sc", "Inc", "Ltd", "Co", "Corp", "No", "Fig",
            "Prof", "Assn", "Dept", "Univ", "Rep", "Sen", "Gov", "Pres", "St", "Mt", "Rd",
            "Calif", "Tex", "N.Y", "D.C"
        },
        StringComparer.OrdinalIgnoreCase
    );

        static readonly Regex _sentenceEndRegex = new Regex(@"(?<=[.!?])\s+", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        public static IEnumerable<string> AsSentences(this string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                yield break;

            var lines = inputText.Split(new[] { '\n' }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                var text = line.Trim();
                if (text.Length == 0) continue;

                int lastIndex = 0;

                foreach (Match match in _sentenceEndRegex.Matches(text))
                {
                    int nextIndex = match.Index + match.Length;
                    string candidate = text.Substring(lastIndex, nextIndex - lastIndex).Trim();

                    if (IsAbbreviation(candidate))
                        continue;

                    if (!string.IsNullOrWhiteSpace(candidate))
                        yield return candidate;

                    lastIndex = nextIndex;
                }

                if (lastIndex < text.Length)
                {
                    string remaining = text.Substring(lastIndex).Trim();
                    if (!string.IsNullOrWhiteSpace(remaining))
                        yield return remaining;
                }
            }
        }

        static bool IsAbbreviation(string candidate)
        {
            var words = candidate.Split(' ');
            if (words.Length == 0) return false;

            string lastWord = words[words.Length - 1].TrimEnd('.', '!', '?');
            return _abbreviations.Contains(lastWord);
        }
    }
}
