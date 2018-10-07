using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Olive.Globalization
{
    partial class Translator
    {
        public static async Task<string> TranslateHtml(string htmlInDefaultLanguage)
            => await TranslateHtml(htmlInDefaultLanguage, await Context.Current.Language());

        public static async Task<string> TranslateHtml(string htmlInDefaultLanguage, ILanguage language)
        {
            if (language == null) throw new ArgumentNullException(nameof(language));
            if (Providers.None()) return htmlInDefaultLanguage;

            var length = Providers.MinOrDefault(x => x.MaximumTextLength);
            if (length == 0) throw new Exception("At least one of the translation providers has 0 MaximumTextLength!");

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlInDefaultLanguage);

            var docNode = htmlDoc.DocumentNode;
            await TranslateNode(docNode, language, length);

            return docNode.OuterHtml;
        }

        static async Task TranslateNode(HtmlNode node, ILanguage language, int maxLength)
        {
            if (node.InnerHtml.Length == 0 ||
                (node.NodeType == HtmlNodeType.Text &&
                !Regex.IsMatch(node.InnerHtml, @"\w+" /* whitespaces */, RegexOptions.Multiline)))
                return;

            if (node.Name == "img")
            {
                var alt = node.Attributes["alt"];
                if (alt != null)
                    alt.Value = await Translate(alt.Value, language);
            }

            if (!node.HasChildNodes && node.InnerHtml.Length <= maxLength)
            {
                node.InnerHtml = await Translate(node.InnerHtml, language);
                return;
            }
            else if (node.ChildNodes.Count > 0)
            {
                foreach (var child in node.ChildNodes)
                    await TranslateNode(child, language, maxLength);
            }
            else
            {
                var lines = BreakDown(node.InnerHtml, maxLength);
                var sb = new StringBuilder();

                foreach (var line in lines)
                    sb.Append(await Translate(line, language));

                node.InnerHtml = sb.ToString();
                return;
            }
        }

        static string[] BreakDown(string text, int eachLineLength)
        {
            text = text.Replace("\n\r", "\n");
            var splites = new[] { '\n', ' ', '.', ',', ';', '!', '?' };

            var resultLines = new List<string>();

            var currentLine = new StringBuilder();

            for (var i = 0; i < text.Length; i++)
            {
                if (currentLine.Length <= eachLineLength)
                {
                    currentLine.Append(text[i]);
                }
                else // currentLineLength > eachLineLength
                {
                    while (!splites.Contains(currentLine[currentLine.Length - 1])/* last char is not splitter*/)
                    {
                        currentLine.Remove(currentLine.Length - 1, 1); // remove last char
                        i--;
                    }

                    i--;
                    resultLines.Add(currentLine.ToString());
                    currentLine = new StringBuilder();
                }
            }

            return resultLines.ToArray();
        }
    }
}
