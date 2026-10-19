namespace Olive.Entities.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>A part of a report query's lambda that has no SQL translation.</summary>
    class ReportTranslationException : NotSupportedException
    {
        public ReportTranslationException(string message) : base(message) { }
    }

    /// <summary>A translated report query: its SQL, its parameters, and how to turn a row into a result.</summary>
    class ReportTranslation<TResult>
    {
        internal DataProvider Provider;
        internal string Sql, CountSql;
        internal Dictionary<string, object> Parameters;
        internal Func<object[], TResult> Materialize;
        internal int? TotalColumn;

        DatabaseContext Context(TimeSpan? timeout) => new DatabaseContext(Provider.ConnectionString)
        {
            CommandTimeout = timeout.HasValue ? (int)Math.Ceiling(timeout.Value.TotalSeconds) : DatabaseContext.Current?.CommandTimeout
        };

        internal async Task Run(TimeSpan? timeout, Action<object[]> onRow)
        {
            using (Context(timeout))
            using (var reader = await Provider.Access.ExecuteReader(Sql, CommandType.Text, Provider.GenerateParameters(Parameters)))
                while (reader.Read())
                {
                    var row = new object[reader.FieldCount];
                    reader.GetValues(row);
                    for (var i = 0; i < row.Length; i++) if (row[i] == DBNull.Value) row[i] = null;
                    onRow(row);
                }
        }

        internal async Task RunCount(TimeSpan? timeout, Action<object> onValue)
        {
            using (Context(timeout))
                onValue(await Provider.Access.ExecuteScalar(CountSql, CommandType.Text, Provider.GenerateParameters(Parameters)));
        }
    }

    /// <summary>
    /// Turns the lambdas of a report query into SQL for the query's provider. Values the lambdas capture are
    /// worked out in C# first, and become parameters (or, where SQL needs the same text twice, literals).
    /// Association paths become LEFT JOINs, one per path, so a grouping or a filter on an associated column
    /// reads it the way a hand-written join would.
    /// </summary>
    class ReportTranslator
    {
        readonly DataProvider Root;
        readonly SqlCommandGenerator Generator;
        readonly DatabaseQuery Query;
        readonly Dictionary<string, object> Values = new Dictionary<string, object>();
        readonly List<Join> Joins = new List<Join>();
        readonly HashSet<ParameterExpression> RowParameters = new HashSet<ParameterExpression>();
        readonly HashSet<ParameterExpression> GroupParameters = new HashSet<ParameterExpression>();
        readonly Dictionary<string, string> KeyMembers = new Dictionary<string, string>();
        readonly List<string> KeySql = new List<string>();
        NewExpression CompositeKey;
        string SingleKey;
        bool InlineConstants;

        /// <summary>A joined association. LEFT OUTER unless a condition at the top of the WHERE needs its row,
        /// when INNER is the same rows and leaves the database free to start from either table, as a
        /// hand-written join would.</summary>
        class Join
        {
            public string Path, Alias, Table;
            public DataProvider Provider;
            public Join Parent;
            public bool Required;

            public void Require()
            {
                for (var join = this; join != null; join = join.Parent) join.Required = true;
            }

            public string Sql => $" {(Required ? "INNER" : "LEFT OUTER")} JOIN {Table}";
        }

        public ReportTranslator(DatabaseQuery source)
        {
            Root = source.Provider as DataProvider ??
                throw new NotSupportedException($"{source.Provider?.GetType().Name} does not support report queries.");

            Generator = Root.SqlCommandGenerator as SqlCommandGenerator ??
                throw new NotSupportedException($"{Root.SqlCommandGenerator?.GetType().Name} does not support report queries.");

            EnsureSingleTable(Root);

            // A copy, so that the criteria's parameters land on it and not on the caller's query.
            Query = (DatabaseQuery)source.CloneFor(source.EntityType);
        }

        static ReportTranslationException Unsupported(string message) => new ReportTranslationException(message);

        static void EnsureSingleTable(DataProvider provider)
        {
            if (provider.MetaData.BaseClassesInOrder.Any() || provider.MetaData.DrivedClasses.Any())
                throw new NotSupportedException($"Report queries do not support {provider.EntityType.Name}: its rows span the tables of an inheritance hierarchy.");
        }

        // ================================================================ the command

        public ReportTranslation<TResult> Translate<TResult>(ReportKind kind, LambdaExpression key, LambdaExpression selector,
            LambdaExpression[] rowConditions, LambdaExpression[] conditions, List<LambdaExpression> filters, bool distinct, List<(LambdaExpression Member, bool Descending)> sort,
            int? top, int pageStartIndex, int? pageSize, bool withTotal = false)
        {
            if (kind == ReportKind.Grouping)
            {
                RowParameters.Add(key.Parameters[0]);
                TranslateKey(Evaluator.PartialEval(key.Body));
                RowParameters.Remove(key.Parameters[0]);
                GroupParameters.Add(selector.Parameters[0]);
            }
            else if (kind == ReportKind.Summary) GroupParameters.Add(selector.Parameters[0]);
            else RowParameters.Add(selector.Parameters[0]);

            var projection = new Projection(this);
            var materialize = projection.Build<TResult>(Evaluator.PartialEval(selector.Body));

            var having = conditions.Select(c =>
            {
                GroupParameters.Add(c.Parameters[0]);
                return "(" + Condition(Evaluator.PartialEval(c.Body)) + ")";
            }).ToList();

            var rowFilters = rowConditions.Select(c =>
            {
                RowParameters.Add(c.Parameters[0]);
                try { RequireRowsFor(Evaluator.PartialEval(c.Body)); }
                finally { RowParameters.Remove(c.Parameters[0]); }
                return "(" + RowLambda(c, condition: true) + ")";
            }).ToList();

            // A filter on the result reads the expressions its members were selected as.
            foreach (var filter in filters)
            {
                var condition = "(" + Condition(Evaluator.PartialEval(projection.InTermsOfSelector(filter))) + ")";
                if (kind == ReportKind.Projection) rowFilters.Add(condition);
                else having.Add(condition);
            }

            var orderBy = (sort ?? new List<(LambdaExpression, bool)>())
                .Select(s => projection.ColumnFor(s.Member) + " DESC".OnlyWhen(s.Descending)).ToString(", ");

            var where = Where() + rowFilters.Select(f => " AND " + f).ToString("");

            var command = new ReportCommand
            {
                Distinct = distinct,
                Columns = projection.Columns.Select((c, i) => $"{c} AS {ColumnAlias(i)}").ToString(", ") +
                    // Counted over the result, after any grouping: the groups, not the rows under them.
                    $", COUNT(*) OVER() AS {ColumnAlias(projection.Columns.Count)}".OnlyWhen(withTotal),
                Tables = Root.GetTables() + Joins.Select(j => j.Sql).ToString(""),
                Where = where,
                GroupBy = KeySql.Distinct().ToString(", "),
                Having = having.ToString(" AND "),
                OrderBy = orderBy,
                Top = top,
                PageStartIndex = pageStartIndex,
                PageSize = pageSize
            };

            var parameters = new Dictionary<string, object>(Query.Parameters);
            foreach (var value in Values) parameters[value.Key] = value.Value;

            var countCommand = new ReportCommand
            {
                Distinct = command.Distinct,
                Columns = projection.Columns.Select((c, i) => $"{c} AS {ColumnAlias(i)}").ToString(", "),
                Tables = command.Tables,
                Where = command.Where,
                GroupBy = command.GroupBy,
                Having = command.Having
            };

            return new ReportTranslation<TResult>
            {
                Provider = Root,
                Sql = Generator.GenerateReportCommand(command),
                CountSql = $"SELECT COUNT(*) FROM ({Generator.GenerateReportCommand(countCommand)}) report_rows",
                Parameters = parameters,
                Materialize = materialize,
                TotalColumn = withTotal ? projection.Columns.Count : (int?)null
            };
        }

        static string ColumnAlias(int index) => "rc" + index;

        void TranslateKey(Expression body)
        {
            InlineConstants = true;
            try
            {
                if (body is NewExpression composite && composite.Members != null)
                {
                    CompositeKey = composite;
                    for (var i = 0; i < composite.Arguments.Count; i++)
                    {
                        var sql = Value(composite.Arguments[i]);
                        KeyMembers[composite.Members[i].Name] = sql;
                        KeySql.Add(sql);
                    }
                }
                else
                {
                    SingleKey = Value(body);
                    KeySql.Add(SingleKey);
                }
            }
            finally { InlineConstants = false; }
        }

        // ================================================================ WHERE: the query's own criteria

        SqlConversionContext RootContext() => new SqlConversionContext
        {
            Query = Query,
            Type = Query.EntityType,
            ToSafeId = Generator.SafeId,
            ToSafeAlias = Generator.SafeAlias
        };

        string Where()
        {
            var context = RootContext();
            var parts = new List<string> { $"{Root.MapColumn(PropertyData.DEFAULT_ID_COLUMN)} IS NOT NULL" };

            if (SoftDeleteAttribute.RequiresSoftdeleteQuery(Query.EntityType))
                parts.Add(new Criterion(PropertyData.IS_MARKED_SOFT_DELETED, false).ToSql(context));

            foreach (var criterion in Query.Criteria) parts.Add(Criterion(criterion, context, topLevel: true));

            return " WHERE " + parts.ToString(" AND ");
        }

        /// <summary>A criterion as GetList would have it, except that one on an association path reads the
        /// joined table rather than a subquery. The join's key is tested too, which makes it match exactly
        /// the rows the subquery would (an IS NULL test must not match a row with no associated row).</summary>
        string Criterion(ICriterion criterion, SqlConversionContext context, bool topLevel = false)
        {
            if (criterion == null) return "(1 = 1)";

            if (criterion is BinaryCriterion binary)
            {
                var both = binary.Operator == BinaryOperator.AND && topLevel;
                return $"({Criterion(binary.Left, context, both)} {binary.Operator} {Criterion(binary.Right, context, both)})";
            }

            if (criterion.GetType() != typeof(Criterion) || !criterion.PropertyName.Contains("."))
                return criterion.ToSql(context);

            var path = criterion.PropertyName.Split('.');
            var join = JoinPath(path.Take(path.Length - 1));
            if (topLevel) join.Require();

            var joined = new Criterion(path.Last(), criterion.FilterFunction, criterion.Value).ToSql(new SqlConversionContext
            {
                Query = Query,
                Type = join.Provider.EntityType,
                Alias = join.Alias,
                ToSafeId = Generator.SafeId,
                ToSafeAlias = Generator.SafeAlias
            });

            return $"({join.Alias}.{Generator.SafeId(join.Provider.MetaData.IdColumnName)} IS NOT NULL AND {joined})";
        }

        // ================================================================ columns and joins

        Join JoinPath(IEnumerable<string> associations)
        {
            Join current = null;
            foreach (var name in associations)
                current = EnsureJoin(current, name);

            return current;
        }

        Join EnsureJoin(Join parent, string association)
        {
            var provider = parent?.Provider ?? Root;
            var path = parent == null ? association : parent.Path + "." + association;

            var existing = Joins.FirstOrDefault(j => j.Path == path);
            if (existing != null) return existing;

            var property = provider.MetaData.AssociateProperties.FirstOrDefault(p => p.Name == association) ??
                throw Unsupported($"{provider.EntityType.Name}.{association} is not an association stored in the database.");

            var target = Context.Current.Database().GetProvider(property.AssociateType) as DataProvider ??
                throw Unsupported($"{property.AssociateType.Name} is not in a database a report query can join.");

            if (target.ConnectionString != Root.ConnectionString)
                throw Unsupported($"{property.AssociateType.Name} is in another database than {Root.EntityType.Name}.");

            EnsureSingleTable(target);

            var alias = "j" + (Joins.Count + 1);
            var foreignKey = parent == null ? Root.MapColumn(property.Name) : $"{parent.Alias}.{Generator.SafeId(property.Name)}";

            var join = new Join
            {
                Path = path,
                Alias = alias,
                Provider = target,
                Parent = parent,
                Table = $"{Generator.GetFullTableName(target.MetaData)} AS {Generator.SafeAlias(alias)} " +
                    $"ON {alias}.{Generator.SafeId(target.MetaData.IdColumnName)} = {foreignKey}"
            };

            Joins.Add(join);
            return join;
        }

        /// <summary>The names of a property chain from a row parameter (p.Definition.Resource.Name gives
        /// Definition, Resource, Name), or null when the chain does not start at one.</summary>
        List<string> Chain(Expression e)
        {
            var names = new List<string>();
            e = StripConvert(e);

            while (e is MemberExpression m)
            {
                names.Insert(0, m.Member.Name);
                e = StripConvert(m.Expression);
            }

            return e is ParameterExpression p && RowParameters.Contains(p) && names.Any() ? names : null;
        }

        static bool IsEntity(Expression e) => (StripConvert(e) as MemberExpression)?.Type.IsA<IEntity>() == true;

        /// <summary>Marks INNER the joins that a condition at the top of a row filter cannot match without: one
        /// that compares an associated column with a value (a missing row compares as unknown, so never
        /// matches), or that says the associated row is there (x.Log != null).</summary>
        void RequireRowsFor(Expression condition)
        {
            if (condition.NodeType == ExpressionType.AndAlso)
            {
                RequireRowsFor(((BinaryExpression)condition).Left);
                RequireRowsFor(((BinaryExpression)condition).Right);
                return;
            }

            if (!(condition is BinaryExpression binary)) return;

            foreach (var (side, other) in new[] { (binary.Left, binary.Right), (binary.Right, binary.Left) })
            {
                if (!(StripConvert(other) is ConstantExpression constant)) continue;

                var chain = Chain(side);
                if (chain == null) continue;

                var isNull = constant.Value == null;
                var entity = IsEntity(side);

                var required = binary.NodeType == ExpressionType.NotEqual ? isNull
                    : !isNull && binary.NodeType != ExpressionType.Equal || !isNull && !entity;

                if (!required) continue;

                var path = entity ? chain : chain.Take(chain.Count - 1).ToList();
                if (path.Any()) JoinPath(path).Require();
            }
        }

        /// <summary>The column a property chain from a row parameter reads (p.Key, p.Definition.Resource.Name,
        /// p.Definition.ServiceId), or null when the chain does not start at one.</summary>
        string Column(MemberExpression member)
        {
            var chain = new List<MemberInfo>();
            Expression current = member;

            while (current is MemberExpression m)
            {
                chain.Insert(0, m.Member);
                current = m.Expression;
            }

            while (current is UnaryExpression u && u.NodeType == ExpressionType.Convert) current = u.Operand;

            if (!(current is ParameterExpression parameter) || !RowParameters.Contains(parameter)) return null;

            Join join = null;
            for (var i = 0; i < chain.Count - 1; i++)
                join = EnsureJoin(join, chain[i].Name);

            var provider = join?.Provider ?? Root;
            var last = chain.Last();
            var type = (last as PropertyInfo)?.PropertyType ?? (last as FieldInfo)?.FieldType;

            if (type.IsA<IEntity>())
                throw Unsupported($"{provider.EntityType.Name}.{last.Name} is an entity: read {last.Name}Id for its key, or one of its properties.");

            if (type.IsA<Blob>())
                throw Unsupported($"{provider.EntityType.Name}.{last.Name} is a Blob, which a report query cannot read.");

            if (last.Name == PropertyData.DEFAULT_ID_COLUMN)
                return join == null ? Root.MapColumn(PropertyData.DEFAULT_ID_COLUMN) : $"{join.Alias}.{Generator.SafeId(provider.MetaData.IdColumnName)}";

            string column;
            {
                var property = provider.MetaData.UserDefienedProperties
                    .FirstOrDefault(p => p.PropertyInfo?.Name == last.Name) ??
                    throw Unsupported($"{provider.EntityType.Name}.{last.Name} is not a column in the database (calculated, or not stored).");

                var converter = property.PropertyInfo.GetCustomAttribute<CustomDataConverterAttribute>(inherit: false);
                if (property.PropertyInfo.Defines<EncryptedPropertyAttribute>() || (converter != null && !(converter is EnumDataConverterAttribute)))
                    throw Unsupported($"{provider.EntityType.Name}.{last.Name} is encrypted or converted, so its column cannot be read as it is.");

                column = property.Name;
            }

            return join == null ? Root.MapColumn(column) : $"{join.Alias}.{Generator.SafeId(column)}";
        }

        // ================================================================ values

        /// <summary>A value with no columns in it: a parameter, or a literal where it is safe and needed.</summary>
        string Literal(object value)
        {
            switch (value)
            {
                case null: return "NULL";
                case Enum e: return Convert.ToInt64(e, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case int _:
                case long _:
                case short _:
                case byte _:
                    return Convert.ToString(value, CultureInfo.InvariantCulture);
                case double _:
                case float _:
                    return Generator.ToDouble(Convert.ToDouble(value).ToString("R", CultureInfo.InvariantCulture));
                case decimal d: return d.ToString(CultureInfo.InvariantCulture);
                case string s when InlineConstants: return Generator.StringLiteral(s);
            }

            var name = "rp" + Values.Count;
            Values[name] = value;
            return "@" + name;
        }

        /// <summary>The SQL for a value, or for a condition used as one (as 1 or 0).</summary>
        public string Value(Expression e)
        {
            switch (e)
            {
                case ConstantExpression constant: return Literal(constant.Value);
                case MemberExpression member: return Member(member);
                case UnaryExpression unary: return Unary(unary);
                case ConditionalExpression conditional: return Conditional(conditional);
                case MethodCallExpression call: return Method(call);
                case BinaryExpression binary: return Binary(binary);
            }

            throw Unsupported($"{e} has no SQL translation.");
        }

        string AsValue(string condition) => $"CASE WHEN {condition} THEN 1 ELSE 0 END";

        string Member(MemberExpression member)
        {
            var target = member.Expression;

            if (target != null && Nullable.GetUnderlyingType(target.Type) != null)
            {
                if (member.Member.Name == "Value") return Value(target);
                if (member.Member.Name == "HasValue") return AsValue(Condition(member));
            }

            if (target != null && (target.Type == typeof(DateTime) || target.Type == typeof(DateTime?)))
                switch (member.Member.Name)
                {
                    case nameof(DateTime.Date): return Generator.TruncateToDay(Value(target));
                    case nameof(DateTime.Hour): return Generator.HourOfDay(Value(target));
                    case nameof(DateTime.DayOfWeek): return Generator.DayOfWeek(Value(target));
                    default: throw Unsupported($"DateTime.{member.Member.Name} has no SQL translation.");
                }

            if (target is ParameterExpression group && GroupParameters.Contains(group) && member.Member.Name == "Key")
                return SingleKey ?? throw Unsupported("The key has several members: read each of them (g.Key.Name).");

            if (target is MemberExpression key && key.Member.Name == "Key" &&
                key.Expression is ParameterExpression keyOwner && GroupParameters.Contains(keyOwner))
                return KeyMembers.TryGetValue(member.Member.Name, out var sql) ? sql :
                    throw Unsupported($"The key has no member {member.Member.Name}.");

            return Column(member) ?? throw Unsupported($"{member} has no SQL translation.");
        }

        string Unary(UnaryExpression unary)
        {
            switch (unary.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    return $"(-{Value(unary.Operand)})";
                case ExpressionType.Not when unary.Type == typeof(bool) || unary.Type == typeof(bool?):
                    return AsValue(Condition(unary));
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return Conversion(unary);
                case ExpressionType.Quote:
                    return Value(unary.Operand);
            }

            throw Unsupported($"{unary} has no SQL translation.");
        }

        static bool IsIntegral(Type type) =>
            type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte);

        static Type Underlying(Type type) => Nullable.GetUnderlyingType(type) ?? type;

        string Conversion(UnaryExpression unary)
        {
            var to = Underlying(unary.Type);
            var from = Underlying(unary.Operand.Type);
            var operand = Value(unary.Operand);

            if (to == from || to == typeof(object)) return operand;
            if (to.IsEnum && IsIntegral(from) || from.IsEnum && IsIntegral(to) || IsIntegral(from) && IsIntegral(to)) return operand;
            if ((to == typeof(double) || to == typeof(float)) && (IsIntegral(from) || from == typeof(decimal) || from == typeof(float) || from == typeof(double)))
                return Generator.ToDouble(operand);
            if (to == typeof(decimal) && IsIntegral(from)) return operand;

            throw Unsupported($"The conversion of {from.Name} to {to.Name} in {unary} has no SQL translation.");
        }

        string Conditional(ConditionalExpression conditional)
        {
            if (conditional.Test is ConstantExpression test && test.Value is bool pick)
                return Value(pick ? conditional.IfTrue : conditional.IfFalse);

            var otherwise = conditional.IfFalse is ConstantExpression none && none.Value == null ? "" : $" ELSE {Value(conditional.IfFalse)}";
            return $"CASE WHEN {Condition(conditional.Test)} THEN {Value(conditional.IfTrue)}{otherwise} END";
        }

        string Binary(BinaryExpression binary)
        {
            switch (binary.NodeType)
            {
                case ExpressionType.Add when binary.Type == typeof(string):
                    return Generator.Concat(ConcatParts(binary).Select(Value));
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return $"({Value(binary.Left)} + {Value(binary.Right)})";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return $"({Value(binary.Left)} - {Value(binary.Right)})";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return $"({Value(binary.Left)} * {Value(binary.Right)})";
                case ExpressionType.Divide:
                    return $"({Value(binary.Left)} / {Divisor(binary)})";
                case ExpressionType.Modulo:
                    return $"({Value(binary.Left)} % {Value(binary.Right)})";
                case ExpressionType.Coalesce:
                    return $"COALESCE({Value(binary.Left)}, {Value(binary.Right)})";
            }

            if (IsCondition(binary)) return AsValue(Condition(binary));

            throw Unsupported($"{binary} has no SQL translation.");
        }

        /// <summary>A non-integer divided by an integer keeps the integer as it is, so the database divides in the
        /// left side's own type, as SQL written by hand does: a decimal column stays decimal arithmetic. C# casts
        /// the integer to double first, which as SQL would make the division floating point.</summary>
        string Divisor(BinaryExpression binary)
        {
            var left = Underlying(binary.Left.Type);
            if ((left == typeof(double) || left == typeof(float)) &&
                binary.Right is UnaryExpression convert && convert.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked &&
                IsIntegral(Underlying(convert.Operand.Type)))
                return Value(convert.Operand);

            return Value(binary.Right);
        }

        static IEnumerable<Expression> ConcatParts(Expression e)
        {
            if (e is BinaryExpression b && b.NodeType == ExpressionType.Add && b.Type == typeof(string))
                return ConcatParts(b.Left).Concat(ConcatParts(b.Right));

            if (e is MethodCallExpression call && call.Method.DeclaringType == typeof(string) && call.Method.Name == nameof(string.Concat))
            {
                if (call.Arguments.Count == 1 && call.Arguments[0] is NewArrayExpression array) return array.Expressions.SelectMany(ConcatParts);
                return call.Arguments.SelectMany(ConcatParts);
            }

            if (e is UnaryExpression u && u.NodeType == ExpressionType.Convert && u.Type == typeof(object)) return ConcatParts(u.Operand);

            return new[] { e };
        }

        string Method(MethodCallExpression call)
        {
            var method = call.Method;

            if (method.DeclaringType == typeof(DbFunctions))
                switch (method.Name)
                {
                    case nameof(DbFunctions.TruncateToHour): return Generator.TruncateToHour(Value(call.Arguments[0]));
                    case nameof(DbFunctions.TruncateToMinutes): return Generator.TruncateToMinutes(Value(call.Arguments[0]), ConstantInt(call.Arguments[1], "minutes"));
                    case nameof(DbFunctions.Left): return Generator.Left(Value(call.Arguments[0]), ConstantInt(call.Arguments[1], "length"));
                    case nameof(DbFunctions.NullIfEmpty): return $"NULLIF({Value(call.Arguments[0])}, '')";
                    case nameof(DbFunctions.CaseInsensitive): return Generator.CaseInsensitive(Value(call.Arguments[0]));
                }

            if (method.DeclaringType == typeof(Enumerable) && IsGroupSource(call.Arguments.FirstOrDefault()))
                return Aggregate(call);

            if (method.Name == nameof(object.ToString) && call.Arguments.Count == 0 && call.Object != null &&
                Underlying(call.Object.Type) == typeof(Guid))
                return Generator.GuidAsText(Value(call.Object));

            if (method.DeclaringType == typeof(string))
                switch (method.Name)
                {
                    case nameof(string.Trim) when call.Arguments.Count == 0: return Generator.Trim(Value(call.Object));
                    case nameof(string.ToLower) when call.Arguments.Count == 0:
                    case nameof(string.ToLowerInvariant): return $"LOWER({Value(call.Object)})";
                    case nameof(string.ToUpper) when call.Arguments.Count == 0:
                    case nameof(string.ToUpperInvariant): return $"UPPER({Value(call.Object)})";
                    case nameof(string.Replace) when call.Arguments.Count == 2 && call.Arguments[0].Type == typeof(string):
                        return $"REPLACE({Value(call.Object)}, {Value(call.Arguments[0])}, {Value(call.Arguments[1])})";
                    case nameof(string.Concat): return Generator.Concat(ConcatParts(call).Select(Value));
                }

            if (IsCondition(call)) return AsValue(Condition(call));

            throw Unsupported($"{call} has no SQL translation.");
        }

        static int ConstantInt(Expression e, string name) =>
            e is ConstantExpression c && c.Value is int value ? value : throw Unsupported($"The {name} must be a constant.");

        // ================================================================ conditions

        static bool IsCondition(Expression e)
        {
            switch (e.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    return true;
                case ExpressionType.And:
                case ExpressionType.Or:
                case ExpressionType.Not:
                    return e.Type == typeof(bool) || e.Type == typeof(bool?);
                case ExpressionType.Call:
                    return e.Type == typeof(bool) && IsPredicate((MethodCallExpression)e);
            }

            return false;
        }

        static bool IsPredicate(MethodCallExpression call)
        {
            var name = call.Method.Name;
            if (call.Method.DeclaringType == typeof(DbFunctions)) return name == nameof(DbFunctions.Like) || name == nameof(DbFunctions.LikeAny);

            if (call.Method.DeclaringType == typeof(string))
                return name == nameof(string.Contains) || name == nameof(string.StartsWith) || name == nameof(string.EndsWith) ||
                    name == nameof(string.IsNullOrEmpty) || name == nameof(string.IsNullOrWhiteSpace) || name == nameof(string.Equals);

            return name == "Contains" || name == "IsAnyOf" || name == nameof(Enumerable.Any) || name == nameof(object.Equals);
        }

        /// <summary>The SQL for a condition, or for a boolean value used as one.</summary>
        public string Condition(Expression e)
        {
            switch (e.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.And when e.Type == typeof(bool):
                    return Logical((BinaryExpression)e, "AND", identity: true);
                case ExpressionType.OrElse:
                case ExpressionType.Or when e.Type == typeof(bool):
                    return Logical((BinaryExpression)e, "OR", identity: false);
                case ExpressionType.Not when e.Type == typeof(bool) || e.Type == typeof(bool?):
                    return $"NOT ({Condition(((UnaryExpression)e).Operand)})";
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    return Compare((BinaryExpression)e);
                case ExpressionType.Constant when ((ConstantExpression)e).Value is bool constant:
                    return constant ? "1 = 1" : "1 = 0";
                case ExpressionType.MemberAccess when ((MemberExpression)e).Member.Name == "HasValue" &&
                    Nullable.GetUnderlyingType(((MemberExpression)e).Expression.Type) != null:
                    return $"{Value(((MemberExpression)e).Expression)} IS NOT NULL";
                case ExpressionType.Conditional:
                    var conditional = (ConditionalExpression)e;
                    if (conditional.Test is ConstantExpression test && test.Value is bool pick)
                        return Condition(pick ? conditional.IfTrue : conditional.IfFalse);
                    break;
                case ExpressionType.Call:
                    var predicate = Predicate((MethodCallExpression)e);
                    if (predicate != null) return predicate;
                    break;
            }

            // A boolean column, or a boolean value worked out some other way.
            return $"{Value(e)} = {Literal(true)}";
        }

        /// <summary>AND or OR, leaving out a side that is a known true or false (a C# flag the lambda captured).</summary>
        string Logical(BinaryExpression binary, string @operator, bool identity)
        {
            bool? known(Expression side) => side is ConstantExpression c && c.Value is bool b ? b : (bool?)null;

            foreach (var (side, other) in new[] { (binary.Left, binary.Right), (binary.Right, binary.Left) })
                if (known(side) is bool value)
                    return value == identity ? Condition(other) : (value ? "1 = 1" : "1 = 0");

            return $"({Condition(binary.Left)} {@operator} {Condition(binary.Right)})";
        }

        static Expression StripConvert(Expression e)
        {
            while (e is UnaryExpression u && (u.NodeType == ExpressionType.Convert || u.NodeType == ExpressionType.ConvertChecked)) e = u.Operand;
            return e;
        }

        static bool IsNull(Expression e) => StripConvert(e) is ConstantExpression c && c.Value == null;

        static bool CanBeNull(Type type) => !type.IsValueType || Nullable.GetUnderlyingType(type) != null;

        string Compare(BinaryExpression binary)
        {
            var left = binary.Left;
            var right = binary.Right;

            if (binary.NodeType == ExpressionType.Equal || binary.NodeType == ExpressionType.NotEqual)
            {
                var not = "NOT ".OnlyWhen(binary.NodeType == ExpressionType.NotEqual);
                if (IsNull(right)) return $"{NullTested(left)} IS {not}NULL";
                if (IsNull(left)) return $"{NullTested(right)} IS {not}NULL";
            }

            var l = Value(left);
            var r = Value(right);

            switch (binary.NodeType)
            {
                case ExpressionType.Equal: return $"{l} = {r}";
                case ExpressionType.NotEqual:
                    // As C# reads it: a null is not equal to a value.
                    if (StripConvert(right) is ConstantExpression && CanBeNull(left.Type)) return $"({l} <> {r} OR {l} IS NULL)";
                    if (StripConvert(left) is ConstantExpression && CanBeNull(right.Type)) return $"({l} <> {r} OR {r} IS NULL)";
                    return $"{l} <> {r}";
                case ExpressionType.LessThan: return $"{l} < {r}";
                case ExpressionType.LessThanOrEqual: return $"{l} <= {r}";
                case ExpressionType.GreaterThan: return $"{l} > {r}";
                default: return $"{l} >= {r}";
            }
        }

        /// <summary>What a null test reads: the value, or for an associated entity its joined row's key.</summary>
        string NullTested(Expression e)
        {
            if (!IsEntity(e) || Chain(e) == null) return Value(e);

            var join = JoinPath(Chain(e));
            return $"{join.Alias}.{Generator.SafeId(join.Provider.MetaData.IdColumnName)}";
        }

        static bool IgnoresCase(Expression comparison) =>
            comparison is ConstantExpression c && c.Value is StringComparison value &&
            (value == StringComparison.OrdinalIgnoreCase || value == StringComparison.CurrentCultureIgnoreCase ||
             value == StringComparison.InvariantCultureIgnoreCase);

        string Predicate(MethodCallExpression call)
        {
            var method = call.Method;
            var args = call.Arguments;

            if (method.DeclaringType == typeof(string))
            {
                var caseless = args.Count > 1 && IgnoresCase(args.Last());
                string text() => caseless ? Generator.CaseInsensitive(Value(call.Object)) : Value(call.Object);

                switch (method.Name)
                {
                    case nameof(string.Contains) when call.Object != null && args[0].Type == typeof(string):
                        return $"{Generator.IndexOf(text(), Value(args[0]))} > 0";
                    case nameof(string.StartsWith) when call.Object != null && args[0].Type == typeof(string):
                        return Like(text(), args[0], prefix: false, suffix: true);
                    case nameof(string.EndsWith) when call.Object != null && args[0].Type == typeof(string):
                        return Like(text(), args[0], prefix: true, suffix: false);
                    case nameof(string.IsNullOrEmpty):
                        var value = Value(args[0]);
                        return $"({value} IS NULL OR {value} = '')";
                    case nameof(string.IsNullOrWhiteSpace):
                        var trimmed = Value(args[0]);
                        return $"({trimmed} IS NULL OR {Generator.Trim(trimmed)} = '')";
                    case nameof(string.Equals) when call.Object != null:
                        return $"{(caseless ? Generator.CaseInsensitive(Value(call.Object)) : Value(call.Object))} = {Value(args[0])}";
                    case nameof(string.Equals) when call.Object == null && args.Count >= 2:
                        return $"{(caseless ? Generator.CaseInsensitive(Value(args[0])) : Value(args[0]))} = {Value(args[1])}";
                }
            }

            if (method.DeclaringType == typeof(DbFunctions))
                switch (method.Name)
                {
                    case nameof(DbFunctions.Like):
                        return $"{Value(args[0])} LIKE {Value(args[1])} ESCAPE '\\'";
                    case nameof(DbFunctions.LikeAny):
                        var patterns = (StripConvert(args[1]) as ConstantExpression)?.Value as IEnumerable ??
                            throw Unsupported($"The patterns in {call} must be a known list.");
                        var text = Value(args[0]);
                        var any = patterns.Cast<string>().Select(p => $"{text} LIKE {Literal(p)} ESCAPE '\\'").ToList();
                        return any.None() ? "1 = 0" : "(" + any.ToString(" OR ") + ")";
                }

            if (method.Name == nameof(Enumerable.Any) && method.DeclaringType == typeof(Enumerable) && IsGroupSource(args[0]))
                return AggregateAny(call);

            if (method.Name == nameof(object.Equals) && call.Object != null && args.Count == 1)
                return Compare(Expression.Equal(call.Object, Expression.Convert(args[0], call.Object.Type)));

            // list.Contains(x), Enumerable.Contains(list, x), x.IsAnyOf(list or values)
            if (method.Name == "Contains")
            {
                if (call.Object != null && args.Count == 1 && !(call.Object.Type == typeof(string))) return In(args[0], call.Object);
                if (call.Object == null && args.Count == 2) return In(args[1], args[0]);
            }

            if (method.Name == "IsAnyOf" && call.Object == null && args.Count == 2) return In(args[0], args[1]);

            return null;
        }

        string Like(string text, Expression pattern, bool prefix, bool suffix)
        {
            if (!(pattern is ConstantExpression constant) || !(constant.Value is string value))
                throw Unsupported($"The text to look for in {pattern} must be a known value.");

            var escaped = value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
            return $"{text} LIKE {Literal("%".OnlyWhen(prefix) + escaped + "%".OnlyWhen(suffix))} ESCAPE '\\'";
        }

        /// <summary>An IN list of known values, written as literals where they are numbers or keys (a list can
        /// be long, and parameters run out) and as parameters otherwise.</summary>
        string In(Expression item, Expression list)
        {
            list = StripConvert(list);

            // C# 14 turns array.Contains(x) into MemoryExtensions.Contains(span, x): read the array itself.
            if (list is MethodCallExpression implicitSpan && implicitSpan.Method.Name == "op_Implicit") list = implicitSpan.Arguments[0];

            if (!(list is ConstantExpression constant))
                throw Unsupported($"The list in {list} must be a known value.");

            if (constant.Value == null) return "1 = 0";

            var values = ((IEnumerable)constant.Value).Cast<object>().ToList();
            var includesNull = values.Contains(null);
            values = values.Where(x => x != null).Distinct().ToList();

            var column = Value(item);
            if (values.None()) return includesNull ? $"{column} IS NULL" : "1 = 0";

            var literals = values.Select(v =>
            {
                switch (v)
                {
                    case Guid guid: return $"'{guid}'";
                    case Enum e: return Convert.ToInt64(e, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                    case int _: case long _: case short _: case byte _: return Convert.ToString(v, CultureInfo.InvariantCulture);
                    case string text: return Generator.StringLiteral(text);
                    default: return Literal(v);
                }
            }).ToString(", ");

            var result = $"{column} IN ({literals})";
            return includesNull ? $"({result} OR {column} IS NULL)" : result;
        }

        // ================================================================ aggregates

        bool IsGroupSource(Expression e)
        {
            while (e is MethodCallExpression call && call.Method.DeclaringType == typeof(Enumerable) && call.Arguments.Any())
                e = call.Arguments[0];

            return e is ParameterExpression parameter && GroupParameters.Contains(parameter);
        }

        class AggregateSource
        {
            public List<LambdaExpression> Conditions = new List<LambdaExpression>();
            public LambdaExpression Selector;
            public bool Distinct;
        }

        AggregateSource Source(Expression e)
        {
            if (e is ParameterExpression parameter && GroupParameters.Contains(parameter)) return new AggregateSource();

            if (e is MethodCallExpression call && call.Method.DeclaringType == typeof(Enumerable))
            {
                var source = Source(call.Arguments[0]);
                switch (call.Method.Name)
                {
                    case nameof(Enumerable.Where) when call.Arguments.Count == 2:
                        if (source.Selector != null) throw Unsupported($"Filter the rows before Select(): {call}.");
                        source.Conditions.Add(Lambda(call.Arguments[1], 1));
                        return source;
                    case nameof(Enumerable.Select) when call.Arguments.Count == 2:
                        if (source.Selector != null) throw Unsupported($"Only one Select() is supported: {call}.");
                        source.Selector = Lambda(call.Arguments[1], 1);
                        return source;
                    case nameof(Enumerable.Distinct) when call.Arguments.Count == 1:
                        if (source.Selector == null) throw Unsupported($"Distinct() needs a Select() before it: {call}.");
                        source.Distinct = true;
                        return source;
                }
            }

            throw Unsupported($"{e} is not something a group can be aggregated through.");
        }

        static LambdaExpression Lambda(Expression e, int parameters)
        {
            while (e is UnaryExpression u && u.NodeType == ExpressionType.Quote) e = u.Operand;

            if (e is LambdaExpression lambda && lambda.Parameters.Count == parameters) return lambda;
            throw Unsupported($"{e} must be a lambda with {parameters} parameter.");
        }

        string RowLambda(LambdaExpression lambda, bool condition)
        {
            var parameter = lambda.Parameters[0];
            RowParameters.Add(parameter);
            try
            {
                var body = Evaluator.PartialEval(lambda.Body);
                return condition ? Condition(body) : Value(body);
            }
            finally { RowParameters.Remove(parameter); }
        }

        string Matching(AggregateSource source) =>
            source.Conditions.Select(c => "(" + RowLambda(c, condition: true) + ")").ToString(" AND ");

        string Aggregate(MethodCallExpression call)
        {
            var name = call.Method.Name;
            var source = Source(call.Arguments[0]);
            var terminal = call.Arguments.Count > 1 ? Lambda(call.Arguments[1], 1) : null;

            switch (name)
            {
                case nameof(Enumerable.Count):
                case nameof(Enumerable.LongCount):
                    if (terminal != null) source.Conditions.Add(terminal);
                    var matching = Matching(source);

                    if (source.Distinct)
                    {
                        var value = RowLambda(source.Selector, condition: false);
                        return $"COUNT(DISTINCT {(matching.HasValue() ? $"CASE WHEN {matching} THEN {value} END" : value)})";
                    }

                    return matching.HasValue() ? $"COUNT(CASE WHEN {matching} THEN 1 END)" : "COUNT(*)";

                case nameof(Enumerable.Sum):
                case nameof(Enumerable.Min):
                case nameof(Enumerable.Max):
                case nameof(Enumerable.Average):
                    if (terminal != null && source.Selector != null) throw Unsupported($"Select() and a selector together are not supported: {call}.");

                    var selector = terminal ?? source.Selector ?? throw Unsupported($"{name}() needs a selector: {call}.");
                    var sql = RowLambda(selector, condition: false);

                    if (name == nameof(Enumerable.Average) && (IsIntegral(Underlying(selector.Body.Type)) || Underlying(selector.Body.Type) == typeof(decimal)))
                        sql = Generator.ToDouble(sql);

                    var condition = Matching(source);
                    if (condition.HasValue()) sql = $"CASE WHEN {condition} THEN {sql} END";

                    var function = name == nameof(Enumerable.Sum) ? "SUM" : name == nameof(Enumerable.Min) ? "MIN" :
                        name == nameof(Enumerable.Max) ? "MAX" : "AVG";

                    return $"{function}({"DISTINCT ".OnlyWhen(source.Distinct)}{sql})";
            }

            if (name == nameof(Enumerable.Any)) return AsValue(AggregateAny(call));

            throw Unsupported($"{name}() has no SQL translation in a report query.");
        }

        string AggregateAny(MethodCallExpression call)
        {
            var source = Source(call.Arguments[0]);
            if (call.Arguments.Count > 1) source.Conditions.Add(Lambda(call.Arguments[1], 1));

            var matching = Matching(source);
            return matching.HasValue() ? $"COUNT(CASE WHEN {matching} THEN 1 END) > 0" : "COUNT(*) > 0";
        }

        // ================================================================ the result

        /// <summary>
        /// The result's shape: every part of the selector that SQL can compute becomes a column, and what is
        /// left (a constructor, a lookup, a format) is compiled into a function that runs on the row read.
        /// </summary>
        class Projection : ExpressionVisitor
        {
            readonly ReportTranslator Translator;
            readonly ParameterExpression Row = Expression.Parameter(typeof(object[]), "row");
            internal readonly List<string> Columns = new List<string>();
            readonly List<Expression> Sources = new List<Expression>();
            Expression Body;

            public Projection(ReportTranslator translator) => Translator = translator;

            public Func<object[], TResult> Build<TResult>(Expression body)
            {
                Body = body;
                var rewritten = Visit(body);

                var left = new ParameterFinder(Translator).Find(rewritten);
                if (left != null) throw Unsupported($"{left} has no SQL translation.");

                if (Columns.None()) throw Unsupported("A report query must read at least one value from the database.");

                return Materializers.For<TResult>(rewritten, Row);
            }

            public override Expression Visit(Expression node)
            {
                if (node == null) return null;
                if (node is LambdaExpression) return node;
                if (new ParameterFinder(Translator).Find(node) == null) return node;

                if (Translator.IsCompositeKey(node))
                    return Expression.New(Translator.CompositeKey.Constructor,
                        Translator.CompositeKey.Arguments.Select((a, i) => Read(Translator.KeyMembers[Translator.CompositeKey.Members[i].Name], a, a.Type)),
                        Translator.CompositeKey.Members);

                var sql = Translator.TryValue(node);
                if (sql != null) return Read(sql, node, node.Type);

                return base.Visit(node);
            }

            Expression Read(string sql, Expression source, Type type)
            {
                Columns.Add(sql);
                Sources.Add(source);

                var read = typeof(ReportValue).GetMethod(nameof(ReportValue.Read)).MakeGenericMethod(type);
                return Expression.Call(read, Row, Expression.Constant(Columns.Count - 1));
            }

            /// <summary>The column a sort by a result member reads: the member's own column.</summary>
            public string ColumnFor(LambdaExpression member)
            {
                var body = StripConvert(member.Body);
                Expression source;

                if (body == member.Parameters[0]) source = Body;
                else if (body is MemberExpression m && m.Expression == member.Parameters[0]) source = Binding(m.Member);
                else throw Unsupported($"Sort by a member of the result: {member}.");

                var index = Sources.IndexOf(source);
                if (index < 0) throw Unsupported($"{member} is worked out in C#, so the database cannot sort by it.");

                return ColumnAlias(index);
            }

            /// <summary>A lambda on the result, rewritten on what the selector reads: r.Total becomes the
            /// expression Total was selected as.</summary>
            public Expression InTermsOfSelector(LambdaExpression lambda) => new ResultMembers(this, lambda.Parameters[0]).Visit(lambda.Body);

            class ResultMembers : ExpressionVisitor
            {
                readonly Projection Projection;
                readonly ParameterExpression Result;

                public ResultMembers(Projection projection, ParameterExpression result)
                {
                    Projection = projection;
                    Result = result;
                }

                protected override Expression VisitMember(MemberExpression node) =>
                    node.Expression == Result ? Projection.Binding(node.Member) : base.VisitMember(node);

                protected override Expression VisitParameter(ParameterExpression node) => node == Result ? Projection.Body : node;
            }

            Expression Binding(MemberInfo member)
            {
                if (Body is NewExpression @new && @new.Members != null)
                {
                    var index = @new.Members.Select(x => x.Name).ToList().IndexOf(member.Name);
                    if (index >= 0) return @new.Arguments[index];
                }

                if (Body is MemberInitExpression init)
                {
                    var binding = init.Bindings.OfType<MemberAssignment>().FirstOrDefault(b => b.Member.Name == member.Name);
                    if (binding != null) return binding.Expression;
                }

                throw Unsupported($"The result has no member {member.Name} set by the selector.");
            }
        }

        bool IsCompositeKey(Expression e) =>
            CompositeKey != null && e is MemberExpression m && m.Member.Name == "Key" &&
            m.Expression is ParameterExpression p && GroupParameters.Contains(p);

        /// <summary>The SQL for an expression, or null (with nothing it added kept) when it has none.</summary>
        string TryValue(Expression e)
        {
            var joins = Joins.Count;
            var values = Values.Keys.ToList();

            try { return Value(e); }
            catch (ReportTranslationException)
            {
                Joins.RemoveRange(joins, Joins.Count - joins);
                foreach (var added in Values.Keys.Except(values).ToList()) Values.Remove(added);
                return null;
            }
        }

        /// <summary>Finds a reference to a row or group parameter, which C# could not evaluate on its own.</summary>
        class ParameterFinder : ExpressionVisitor
        {
            readonly ReportTranslator Translator;
            Expression Found;

            public ParameterFinder(ReportTranslator translator) => Translator = translator;

            public Expression Find(Expression e)
            {
                Found = null;
                Visit(e);
                return Found;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (Found == null && (Translator.RowParameters.Contains(node) || Translator.GroupParameters.Contains(node))) Found = node;
                return node;
            }
        }

        /// <summary>Evaluates in C# every part of an expression that does not depend on the lambda's
        /// parameters, such as captured variables, so the translator sees their values.</summary>
        static class Evaluator
        {
            public static Expression PartialEval(Expression e) => new SubtreeEvaluator(new Nominator().Nominate(e)).Visit(e);

            class SubtreeEvaluator : ExpressionVisitor
            {
                readonly HashSet<Expression> Candidates;

                public SubtreeEvaluator(HashSet<Expression> candidates) => Candidates = candidates;

                public override Expression Visit(Expression node)
                {
                    if (node == null) return null;
                    if (Candidates.Contains(node)) return Evaluate(node);
                    return base.Visit(node);
                }

                // An initializer's constructor must stay a constructor, even when it takes no row values.
                protected override Expression VisitMemberInit(MemberInitExpression node) =>
                    node.Update(node.NewExpression, node.Bindings.Select(VisitMemberBinding));

                static Expression Evaluate(Expression e)
                {
                    if (e.NodeType == ExpressionType.Constant) return e;
                    return Expression.Constant(TryRead(e, out var value) ? value : Expression.Lambda(e).Compile().DynamicInvoke(null), e.Type);
                }

                /// <summary>A captured variable (a field or property chain off a constant, perhaps converted) read
                /// by reflection: compiling a delegate for each would cost more than the query.</summary>
                static bool TryRead(Expression e, out object value)
                {
                    value = null;
                    switch (e)
                    {
                        case ConstantExpression constant:
                            value = constant.Value;
                            return true;
                        case MemberExpression member when member.Expression == null || TryRead(member.Expression, out value):
                            if (member.Expression != null && value == null) return false;
                            if (member.Member is FieldInfo field) { value = field.GetValue(value); return true; }
                            if (member.Member is PropertyInfo property && property.GetIndexParameters().Length == 0)
                            {
                                value = property.GetValue(value);
                                return true;
                            }
                            return false;
                        case UnaryExpression convert when convert.NodeType == ExpressionType.Convert && convert.Method == null &&
                            TryRead(convert.Operand, out value):
                            var target = Nullable.GetUnderlyingType(convert.Type) ?? convert.Type;
                            if (value == null || target.IsInstanceOfType(value)) return true;
                            if (target.IsEnum || value is Enum || !(value is IConvertible)) return false;
                            value = System.Convert.ChangeType(value, target, CultureInfo.InvariantCulture);
                            return true;
                    }

                    return false;
                }
            }

            class Nominator : ExpressionVisitor
            {
                readonly HashSet<Expression> Candidates = new HashSet<Expression>();
                bool CannotBeEvaluated;

                public HashSet<Expression> Nominate(Expression e)
                {
                    Visit(e);
                    return Candidates;
                }

                static bool IsRefStruct(Type type) =>
                    type.GetCustomAttributes(false).Any(a => a.GetType().FullName == "System.Runtime.CompilerServices.IsByRefLikeAttribute");

                public override Expression Visit(Expression node)
                {
                    if (node == null) return null;

                    var saved = CannotBeEvaluated;
                    CannotBeEvaluated = false;
                    base.Visit(node);

                    if (!CannotBeEvaluated)
                    {
                        if (node.NodeType == ExpressionType.Parameter || node.NodeType == ExpressionType.Lambda ||
                            node.NodeType == ExpressionType.Quote || IsRefStruct(node.Type) ||
                            (node is MethodCallExpression call && call.Method.DeclaringType == typeof(DbFunctions)))
                            CannotBeEvaluated = true;
                        else Candidates.Add(node);
                    }

                    CannotBeEvaluated |= saved;
                    return node;
                }
            }
        }
    }

    /// <summary>
    /// The compiled functions that turn a row into a result, one per shape of selector: compiling one for every
    /// run would cost more than a small query. The values the selector captured are taken out of the shape and
    /// passed in, so a query run again with other values reuses what the first run compiled.
    /// </summary>
    static class Materializers
    {
        static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Delegate> Cache =
            new System.Collections.Concurrent.ConcurrentDictionary<string, Delegate>();

        public static Func<object[], TResult> For<TResult>(Expression body, ParameterExpression row)
        {
            var values = Expression.Parameter(typeof(object[]), "values");
            var lifted = new Lifter(values);
            var shape = lifted.Visit(Expression.Convert(body, typeof(TResult)));

            var key = typeof(TResult).AssemblyQualifiedName + "|" + Shape.Of(shape);
            var compiled = (Func<object[], object[], TResult>)Cache.GetOrAdd(key,
                _ => Expression.Lambda<Func<object[], object[], TResult>>(shape, values, row).Compile());

            var captured = lifted.Values.ToArray();
            return r => compiled(captured, r);
        }

        /// <summary>Replaces every constant with a read of the values array.</summary>
        class Lifter : ExpressionVisitor
        {
            readonly ParameterExpression ValuesArray;
            public readonly List<object> Values = new List<object>();

            public Lifter(ParameterExpression values) => ValuesArray = values;

            protected override Expression VisitConstant(ConstantExpression node)
            {
                Values.Add(node.Value);
                return Expression.Convert(Expression.ArrayIndex(ValuesArray, Expression.Constant(Values.Count - 1)), node.Type);
            }
        }

        /// <summary>A text that two expressions share only when they compile to the same function: every node's
        /// kind and type, and the methods, members and constructors it uses.</summary>
        class Shape : ExpressionVisitor
        {
            readonly System.Text.StringBuilder Text = new System.Text.StringBuilder();
            readonly Dictionary<ParameterExpression, int> Parameters = new Dictionary<ParameterExpression, int>();

            public static string Of(Expression e)
            {
                var shape = new Shape();
                shape.Visit(e);
                return shape.Text.ToString();
            }

            static string Name(MemberInfo member) =>
                member == null ? "" : member.DeclaringType?.AssemblyQualifiedName + "::" + member;

            public override Expression Visit(Expression node)
            {
                if (node == null) { Text.Append("~"); return null; }

                Text.Append('(').Append(node.NodeType).Append(':').Append(node.Type.AssemblyQualifiedName);
                switch (node)
                {
                    case MethodCallExpression call: Text.Append('|').Append(Name(call.Method)); break;
                    case MemberExpression member: Text.Append('|').Append(Name(member.Member)); break;
                    case NewExpression @new:
                        Text.Append('|').Append(Name(@new.Constructor));
                        if (@new.Members != null) foreach (var m in @new.Members) Text.Append(',').Append(Name(m));
                        break;
                    case BinaryExpression binary: Text.Append('|').Append(Name(binary.Method)); break;
                    case UnaryExpression unary: Text.Append('|').Append(Name(unary.Method)); break;
                    case ParameterExpression parameter:
                        if (!Parameters.TryGetValue(parameter, out var index)) Parameters[parameter] = index = Parameters.Count;
                        Text.Append("|p").Append(index);
                        break;
                    case ConstantExpression constant: Text.Append('|').Append(constant.Value); break;
                }

                var result = base.Visit(node);
                Text.Append(')');
                return result;
            }

            protected override MemberBinding VisitMemberBinding(MemberBinding node)
            {
                Text.Append("[").Append(node.BindingType).Append(Name(node.Member)).Append("]");
                return base.VisitMemberBinding(node);
            }
        }
    }

    /// <summary>The values a report query reads, converted to the C# type the selector gave them.</summary>
    public static class ReportValue
    {
        /// <summary>Column <paramref name="index"/> of a row read, as <typeparamref name="T"/>. For use by the
        /// compiled selector.</summary>
        public static T Read<T>(object[] row, int index) => (T)To(row[index], typeof(T));

        internal static object To(object value, Type type)
        {
            if (value == null || value == DBNull.Value)
                return type.IsValueType && Nullable.GetUnderlyingType(type) == null ? Activator.CreateInstance(type) : null;

            var target = Nullable.GetUnderlyingType(type) ?? type;
            if (target.IsInstanceOfType(value)) return value;

            if (target.IsEnum) return Enum.ToObject(target, Convert.ToInt64(value, CultureInfo.InvariantCulture));

            if (target == typeof(Guid))
            {
                if (value is string text) return Guid.Parse(text);
                if (value is byte[] bytes) return new Guid(bytes);
            }

            if (target == typeof(DateTime))
            {
                if (value is DateTimeOffset offset) return offset.DateTime;
                // A date column comes back from some providers as a DateOnly, which netstandard2.0 cannot name.
                if (value.GetType().FullName == "System.DateOnly")
                {
                    var t = value.GetType();
                    return new DateTime((int)t.GetProperty("Year").GetValue(value), (int)t.GetProperty("Month").GetValue(value), (int)t.GetProperty("Day").GetValue(value));
                }
            }

            if (target == typeof(bool)) return Convert.ToInt64(value, CultureInfo.InvariantCulture) != 0;
            if (target == typeof(string)) return Convert.ToString(value, CultureInfo.InvariantCulture);

            return Convert.ChangeType(value, target, CultureInfo.InvariantCulture);
        }
    }
}
