using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Olive.Markdown
{
    public class TagMaker
    {
        public TagMaker(string formatString, Func<XElement, string, IEnumerable<string>> valueExtractor)
        {
            FormatString = formatString;
            ValueExtractor = valueExtractor;
        }

        public string FormatString { get; } = "";

        public Func<
            XElement, //xml Element to extract from 
            string, //assembly name
            IEnumerable<string> //resultant list of values that will get used with formatString
        > ValueExtractor;

        public static Dictionary<string, TagMaker> Dict { get; } = new Dictionary<String, TagMaker>()
        {
            ["doc"] = new TagMaker(
                "# {0} #\n\n{1}\n\n",
                (x, assemblyName) => new[]{
                        x.Element("assembly").Element("name").Value,
                        x.Element("members").Elements("member").ToMarkDown(x.Element("assembly").Element("name").Value)
                }
            ),
            ["type"] = new TagMaker(
                "## {0}\n\n{1}\n\n---\n",
                (x, assemblyName) => XmlToMarkdown.ExtractNameAndBodyFromMember(x, assemblyName)
            ),
            ["field"] = new TagMaker(
                "#### {0}\n\n{1}\n\n---\n",
                (x, assemblyName) => XmlToMarkdown.ExtractNameAndBodyFromMember(x, assemblyName)
            ),
            ["property"] = new TagMaker(
                "#### {0}\n\n{1}\n\n---\n",
                (x, assemblyName) => XmlToMarkdown.ExtractNameAndBodyFromMember(x, assemblyName)
            ),
            ["method"] = new TagMaker(
                "#### {0}\n\n{1}\n\n---\n",
                (x, assemblyName) => XmlToMarkdown.ExtractNameAndBodyFromMember(x, assemblyName)
            ),
            ["event"] = new TagMaker(
                "#### {0}\n\n{1}\n\n---\n",
                (x, assemblyName) => XmlToMarkdown.ExtractNameAndBodyFromMember(x, assemblyName)
            ),
            ["summary"] = new TagMaker(
                "{0}\n\n",
                (x, assemblyName) => new[] { x.Nodes().ToMarkDown(assemblyName) }
            ),
            ["value"] = new TagMaker(
                "**Value**: {0}\n\n",
                (x, assemblyName) => new[] { x.Nodes().ToMarkDown(assemblyName) }
            ),
            ["remarks"] = new TagMaker(
                "\n\n>{0}\n\n",
                (x, assemblyName) => new[] { x.Nodes().ToMarkDown(assemblyName) }
            ),
            ["example"] = new TagMaker(
                "##### Example: {0}\n\n",
                (x, assemblyName) => new[] { x.Nodes().ToMarkDown(assemblyName) }
            ),
            ["para"] = new TagMaker(
                "{0}\n\n",
                (x, assemblyName) => new[] { x.Nodes().ToMarkDown(assemblyName) }
            ),
            ["code"] = new TagMaker(
                "\n\n###### {0} code\n\n```\n{1}\n```\n\n",
                (x, assemblyName) => new[] { x.Attribute("lang")?.Value ?? "", x.Value.ToCodeBlock() }
            ),
            ["seePage"] = new TagMaker(
                "[[{1}|{0}]]",
                (x, assemblyName) => XmlToMarkdown.ExtractNameAndBody("cref", x, assemblyName)
            ),
            ["seeAnchor"] = new TagMaker(
                "[{1}]({0})]",
                (x, assemblyName) => { var xx = XmlToMarkdown.ExtractNameAndBody("cref", x, assemblyName); xx[0] = xx[0].ToLower(); return xx; }
            ),
            ["firstparam"] = new TagMaker(
                "|Name | Description |\n|-----|------|\n|{0}: |{1}|\n",
                (x, assemblyName) => XmlToMarkdown.ExtractNameAndBody("name", x, assemblyName)
            ),
            ["typeparam"] = new TagMaker(
                "|{0}: |{1}|\n",
                (x, assemblyName) => XmlToMarkdown.ExtractNameAndBody("name", x, assemblyName)
            ),
            ["param"] = new TagMaker(
                "|{0}: |{1}|\n",
                (x, assemblyName) => XmlToMarkdown.ExtractNameAndBody("name", x, assemblyName)
            ),
            ["paramref"] = new TagMaker(
                "`{0}`",
                (x, assemblyName) => XmlToMarkdown.ExtractNameAndBody("name", x, assemblyName)
            ),
            ["exception"] = new TagMaker(
                "[[{0}|{0}]]: {1}\n\n",
                (x, assemblyName) => XmlToMarkdown.ExtractNameAndBody("cref", x, assemblyName)
            ),
            ["returns"] = new TagMaker(
                "**Returns**: {0}\n\n",
                (x, assemblyName) => new[] { x.Nodes().ToMarkDown(assemblyName) }
            ),
            ["c"] = new TagMaker(
                " `{0}` ",
                (x, assemblyName) => new[] { x.Nodes().ToMarkDown(assemblyName) }
            ),
            ["none"] = new TagMaker(
                "",
                (x, assemblyName) => new string[0]
            ),
        };
    }
}
