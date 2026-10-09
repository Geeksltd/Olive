using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Olive.Entities.Data
{
    /// <summary>
    /// Translates the T-SQL table scripts M# generates (DB\Tables, DB\@Create.Database.sql) into
    /// PostgreSQL. Only the shapes M# emits are understood; anything else is refused rather than
    /// passed through half translated.
    /// </summary>
    public static class PostgreSqlScriptTranslator
    {
        /// <summary>
        /// The case-insensitive collation string columns are created with. It is the one a SQL Server
        /// database migrated to PostgreSQL carries, so keys between old and new columns stay comparable.
        /// </summary>
        public const string Collation = "mssql.ci_as";

        public static readonly string[] CollationSetup =
        {
            "CREATE SCHEMA IF NOT EXISTS mssql",
            "CREATE COLLATION IF NOT EXISTS mssql.ci_as (provider = icu, locale = 'und-u-ks-level2', deterministic = false)"
        };

        const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.Singleline;

        static readonly Regex[] Skipped =
        {
            // Database-level settings and SQL Server bookkeeping, none of which has a PostgreSQL meaning.
            new Regex(@"^(CREATE|ALTER)\s+DATABASE\b", Options),
            new Regex(@"^USE\s", Options),
            new Regex(@"^EXEC\s+sp_(addextendedproperty|changedbowner)\b", Options),
            // Customize.Database.sql's maintenance procedures are written against SQL Server catalogs.
            new Regex(@"^CREATE\s+PROC(EDURE)?\b", Options)
        };

        // M# guards each schema with this block, with no statement separator before the CREATE TABLE
        // that follows. Schemas are created with the tables that live in them instead.
        static readonly Regex SchemaGuard = new Regex(@"IF\s*\(*\s*NOT\s+EXISTS\s*\(\s*SELECT\b.*?sys\.schemas.*?\bEND\b", Options);

        static readonly Regex CreateTable = new Regex(@"^CREATE\s+TABLE\s+(?<name>[^\s(]+)\s*\((?<body>.*)\)$", Options);

        static readonly Regex CreateIndex = new Regex(
            @"^CREATE\s+(?<unique>UNIQUE\s+)?INDEX\s+(?<name>\[[^\]]+\]|""[^""]+""|\S+)\s+ON\s+(?<table>[^\s(]+)\s*\((?<columns>[^)]*)\)$", Options);

        static readonly Regex AddForeignKey = new Regex(
            @"^ALTER\s+TABLE\s+(?<table>\S+)\s+ADD\s+CONSTRAINT\s+(?<name>\[[^\]]+\]|""[^""]+""|\S+)\s+FOREIGN\s+KEY\s*\((?<columns>[^)]*)\)" +
            @"\s*REFERENCES\s+(?<target>[^\s(]+)\s*\((?<targetColumns>[^)]*)\)(?<actions>.*)$", Options);

        static readonly Regex Column = new Regex(@"^(?<name>\[[^\]]+\]|""[^""]+""|\S+)\s+(?<type>[A-Za-z][A-Za-z0-9_]*(\s*\([^)]*\))?)(?<rest>.*)$", Options);

        static readonly Regex TableConstraint = new Regex(@"^CONSTRAINT\s+(?<name>\[[^\]]+\]|""[^""]+""|\S+)\s+PRIMARY\s+KEY\s*\((?<columns>[^)]*)\)$", Options);

        static readonly Regex InlineReference = new Regex(@"REFERENCES\s+(?<target>[^\s(]+)\s*\((?<columns>[^)]*)\)", Options);

        static readonly Regex ReferentialActions = new Regex(@"^(\s*ON\s+(DELETE|UPDATE)\s+(NO\s+ACTION|CASCADE|SET\s+NULL|SET\s+DEFAULT|RESTRICT))*\s*$", Options);

        /// <summary>The PostgreSQL statements for a T-SQL script, in order.</summary>
        public static IEnumerable<string> Translate(string tsql)
        {
            foreach (var statement in SplitStatements(tsql))
            {
                if (Skipped.Any(x => x.IsMatch(statement))) continue;

                foreach (var result in TranslateStatement(statement))
                    yield return result;
            }
        }

        /// <summary>The PostgreSQL script for a T-SQL script, one statement per paragraph.</summary>
        public static string TranslateToScript(string tsql) =>
            Translate(tsql).Select(x => x + ";").ToString(Environment.NewLine + Environment.NewLine);

        static IEnumerable<string> TranslateStatement(string statement)
        {
            Match match;

            if ((match = CreateTable.Match(statement)).Success)
                return TranslateCreateTable(match);

            if ((match = CreateIndex.Match(statement)).Success)
                return new[]
                {
                    "CREATE " + "UNIQUE ".OnlyWhen(match.Groups["unique"].Success) + "INDEX " + Id(match.Groups["name"].Value) +
                    " ON " + Name(match.Groups["table"].Value) + " (" + Ids(match.Groups["columns"].Value) + ")"
                };

            if ((match = AddForeignKey.Match(statement)).Success)
            {
                var actions = match.Groups["actions"].Value;
                if (!ReferentialActions.IsMatch(actions)) throw Unsupported(statement);

                return new[]
                {
                    "ALTER TABLE " + Name(match.Groups["table"].Value) + " ADD CONSTRAINT " + Id(match.Groups["name"].Value) +
                    " FOREIGN KEY (" + Ids(match.Groups["columns"].Value) + ") REFERENCES " + Name(match.Groups["target"].Value) +
                    " (" + Ids(match.Groups["targetColumns"].Value) + ")" + Normalize(actions).WithPrefix(" ")
                };
            }

            throw Unsupported(statement);
        }

        static IEnumerable<string> TranslateCreateTable(Match match)
        {
            var parts = SplitName(match.Groups["name"].Value);
            if (parts.Length > 1) yield return "CREATE SCHEMA IF NOT EXISTS " + Quote(parts[0]);

            var columns = SplitTopLevel(match.Groups["body"].Value, ',').Select(x => x.Trim()).Where(x => x.HasValue())
                .Select(TranslateTablePart);

            yield return "CREATE TABLE " + Name(match.Groups["name"].Value) + " (" + Environment.NewLine +
                columns.Select(x => "    " + x).ToString("," + Environment.NewLine) + Environment.NewLine + ")";
        }

        static string TranslateTablePart(string part)
        {
            var constraint = TableConstraint.Match(part);
            if (constraint.Success)
                return "CONSTRAINT " + Id(constraint.Groups["name"].Value) + " PRIMARY KEY (" + Ids(constraint.Groups["columns"].Value) + ")";

            var column = Column.Match(part);
            if (!column.Success) throw Unsupported(part);

            var result = new List<string> { Id(column.Groups["name"].Value), TranslateType(column.Groups["type"].Value, part) };
            var rest = Normalize(column.Groups["rest"].Value);

            var identity = Regex.Match(rest, @"\bIDENTITY\s*\(\s*(?<start>\d+)\s*,\s*\d+\s*\)", Options);
            if (identity.Success)
            {
                result.Add($"GENERATED BY DEFAULT AS IDENTITY (START WITH {identity.Groups["start"].Value})");
                rest = rest.Remove(identity.Index, identity.Length);
            }

            var reference = InlineReference.Match(rest);
            var referenceSql = "";
            if (reference.Success)
            {
                referenceSql = "REFERENCES " + Name(reference.Groups["target"].Value) + " (" + Ids(reference.Groups["columns"].Value) + ")";
                rest = rest.Substring(0, reference.Index) + rest.Substring(reference.Index + reference.Length);
            }

            rest = Regex.Replace(rest, @"\bPRIMARY\s+KEY(\s+(NON)?CLUSTERED)?\b", m => { result.Add("PRIMARY KEY"); return " "; }, Options);
            rest = Regex.Replace(rest, @"\bNOT\s+NULL\b", m => { result.Add("NOT NULL"); return " "; }, Options);
            rest = Regex.Replace(rest, @"\bNULL\b", m => { result.Add("NULL"); return " "; }, Options);

            if (referenceSql.HasValue())
            {
                var actions = Regex.Match(rest, @"(\s*ON\s+(DELETE|UPDATE)\s+(NO\s+ACTION|CASCADE|SET\s+NULL|SET\s+DEFAULT|RESTRICT))+", Options);
                if (actions.Success)
                {
                    referenceSql += " " + Normalize(actions.Value);
                    rest = rest.Remove(actions.Index, actions.Length);
                }

                result.Add(referenceSql);
            }

            if (rest.Trim().HasValue()) throw Unsupported(part);

            return result.ToString(" ");
        }

        static string TranslateType(string type, string context)
        {
            var match = Regex.Match(type.Trim(), @"^(?<name>[A-Za-z][A-Za-z0-9_]*)\s*(\((?<size>[^)]*)\))?$", Options);
            var name = match.Groups["name"].Value.ToLowerInvariant();
            var size = match.Groups["size"].Value.Trim();
            var unlimited = size.Equals("max", StringComparison.OrdinalIgnoreCase);

            switch (name)
            {
                case "uniqueidentifier": return "uuid";
                case "nvarchar":
                case "varchar": return (unlimited || size.IsEmpty() ? "text" : $"varchar({size})") + " COLLATE " + Collation;
                case "nchar":
                case "char": return $"char({size.Or("1")}) COLLATE {Collation}";
                case "ntext":
                case "text": return "text COLLATE " + Collation;
                case "bit": return "boolean";
                case "tinyint":
                case "smallint": return "smallint";
                case "int": return "integer";
                case "bigint": return "bigint";
                case "decimal":
                case "numeric": return size.HasValue() ? $"numeric({size})" : "numeric";
                case "money": return "numeric(19, 4)";
                case "smallmoney": return "numeric(10, 4)";
                case "float": return "double precision";
                case "real": return "real";
                case "date": return "date";
                case "time": return "time";
                case "datetime":
                case "datetime2":
                case "smalldatetime": return "timestamp";
                case "datetimeoffset": return "timestamptz";
                case "binary":
                case "varbinary":
                case "image": return "bytea";
                default: throw Unsupported(context);
            }
        }

        /// <summary>An identifier, quoted. The key column is Olive's "ID" whatever case the script used.</summary>
        static string Id(string raw)
        {
            var name = Unquote(raw);
            return Quote(name.Equals("id", StringComparison.OrdinalIgnoreCase) ? "ID" : name);
        }

        static string Ids(string list) => list.Split(',').Select(x => x.Trim()).Where(x => x.HasValue()).Select(Id).ToString(", ");

        /// <summary>A possibly schema-qualified table name. SQL Server's default schema maps to PostgreSQL's.</summary>
        static string Name(string raw)
        {
            var parts = SplitName(raw);
            return parts.Select(Quote).ToString(".");
        }

        static string[] SplitName(string raw) =>
            SplitTopLevel(raw.Trim(), '.').Select(x => Unquote(x.Trim())).Where(x => x.HasValue())
                .SkipWhile((x, i) => i == 0 && x.Equals("dbo", StringComparison.OrdinalIgnoreCase)).ToArray();

        static string Unquote(string raw)
        {
            raw = raw.Trim();
            if (raw.StartsWith("[") && raw.EndsWith("]")) return raw.Substring(1, raw.Length - 2);
            if (raw.StartsWith("\"") && raw.EndsWith("\"")) return raw.Substring(1, raw.Length - 2);
            return raw;
        }

        static string Quote(string name) => "\"" + name.Replace("\"", "\"\"") + "\"";

        static string Normalize(string sql) => Regex.Replace(sql.Trim(), @"\s+", " ");

        static Exception Unsupported(string statement) =>
            new NotSupportedException("Cannot translate this SQL Server statement to PostgreSQL:" + Environment.NewLine + statement.Trim());

        /// <summary>Splits on the separator where it is outside brackets, quotes and parentheses.</summary>
        static IEnumerable<string> SplitTopLevel(string text, char separator)
        {
            var depth = 0;
            var quote = default(char?);
            var current = new StringBuilder();

            foreach (var c in text)
            {
                if (quote.HasValue)
                {
                    if (c == quote) quote = null;
                }
                else if (c == '\'' || c == '"') quote = c;
                else if (c == '[') quote = ']';
                else if (c == '(') depth++;
                else if (c == ')') depth--;
                else if (c == separator && depth == 0)
                {
                    yield return current.ToString();
                    current.Clear();
                    continue;
                }

                current.Append(c);
            }

            if (current.Length > 0) yield return current.ToString();
        }

        static IEnumerable<string> SplitStatements(string tsql)
        {
            var withoutComments = Regex.Replace(tsql.OrEmpty().TrimStart('\uFEFF'), @"--[^\r\n]*", "");
            withoutComments = SchemaGuard.Replace(withoutComments, "");

            foreach (var batch in Regex.Split(withoutComments, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
                foreach (var statement in SplitTopLevel(batch, ';'))
                {
                    var trimmed = statement.Trim().TrimStart('\uFEFF').Trim();
                    if (trimmed.HasValue()) yield return trimmed;
                }
        }
    }
}
