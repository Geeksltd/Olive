using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olive.ApiProxy
{
    class CSharpFormatter
    {
        LinkedList<CodeLine> CodeLines = new LinkedList<CodeLine>();
        int IndentLevel;

        /// <summary>
        /// Creates a new CSharpFormatter instance.
        /// </summary>
        public CSharpFormatter(string source)
        {
            foreach (var line in source.Or("").Trim().Split('\n').Select(l => l.Trim()))
            {
                var item = new CodeLine(line);
                item.Attach(CodeLines.AddLast(item));
            }
        }

        /// <summary>
        /// Formats the specified CSharp code.
        /// </summary>
        public string Format()
        {
            var iterator = CodeLines.FirstOrDefault();
            while (iterator != null) iterator = iterator.InsertEmptyLineBefore();

            iterator = CodeLines.FirstOrDefault();
            while (iterator != null) iterator = iterator.RemoveRedundantEmptyLines();

            foreach (var line in CodeLines)
            {
                line.SetContextIndent(IndentLevel);

                // if (line.Code.EndsWith("=>")) IndentLevel++;
                if (line.Code == "{") IndentLevel++;
                if (line.UnindentsSelf()) IndentLevel--;
            }

            return CodeLines.ToLinesString();
        }

        class CodeLine
        {
            LinkedListNode<CodeLine> Node;
            public string Code;
            string CodeWithAnnotation;
            int ContextIndent;

            internal void Attach(LinkedListNode<CodeLine> node) => Node = node;

            public CodeLine(string code)
            {
                CodeWithAnnotation = code;
                Code = code.TrimAfter("END_H____").Trim().TrimBefore("___END_SH_", trimPhrase: true).Trim();
            }

            CodeLine Previous => Node.Previous?.Value;
            CodeLine Next => Node.Next?.Value;

            public bool NeedsEmptyLineBeforeIt()
            {
                if (Code.IsEmpty() || UnindentsSelf()) return false;

                var prev = Previous?.Code;

                if (prev.IsEmpty() || prev == "{") return false;
                if (prev == "}") return true;
                if (Code == "try") return true;
                if (Code == "scope.Complete();") return true;
                if (Code.StartsWith("try {")) return true;
                if (Code.StartsWith("foreach (")) return true;
                if (Code.StartsWith("if (")) return true;
                if (Code.StartsWith("using (")) return true;

                return false;
            }

            public bool IsRedundantEmptyLine()
            {
                if (Code.HasValue()) return false;

                if (Previous?.Code?.EndsWithAny("{", "[", ",") == true) return true;

                var next = Next?.Code ?? "// END";

                if (next.IsEmpty()) return true;
                if (next.IsAnyOf("else", "}", "{")) return true;
                if (next.StartsWith("else if (")) return true;

                if (next.StartsWith("catch ") || next == "finally") return true;

                return false;
            }

            public bool UnindentsSelf() => Code == "}" || Code == "};" || Code == "});";

            public void SetContextIndent(int indent) => ContextIndent = indent;

            public override string ToString()
            {
                var result = new StringBuilder();

                var indent = EffectiveIndent();
                for (var i = 0; i < indent; i++)
                    result.Append("    ");

                result.Append(CodeWithAnnotation);

                return result.ToString();
            }

            int EffectiveIndent()
            {
                var result = ContextIndent;

                if (Code.StartsWith(".") && !Code.EndsWith("=>")) result++;
                else if (Previous?.IndentsNextLine() == true) result++;

                if (UnindentsSelf()) result--;

                return result.LimitMin(0);
            }

            bool IndentsNextLine()
            {
                var next = Next?.Code; ;

                if (Code.Contains("//")) return false;

                if (next != "{")
                    if (Code == "else" ||
                        (Code.StartsWithAny("for", "if", "while", "else if")) && Code.EndsWith(")"))
                        return true;

                if (Code.EndsWithAny("+", "-", "(", "[")) return true;

                if (Code.StartsWith("case ") && Code.EndsWith(":")) return true;

                if (Code == "default:") return true;

                return false;
            }

            internal CodeLine InsertEmptyLineBefore()
            {
                if (NeedsEmptyLineBeforeIt())
                {
                    var emptyLine = new CodeLine("");
                    emptyLine.Attach(Node.List.AddBefore(Node, emptyLine));
                }

                return Next;
            }

            internal CodeLine RemoveRedundantEmptyLines()
            {
                var next = Next;
                if (IsRedundantEmptyLine()) Node.List.Remove(Node);
                return next;
            }
        }
    }
}