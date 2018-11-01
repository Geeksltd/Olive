using System;
using System.Text;

namespace Olive.Entities.Data
{
    public abstract class DatabaseCriterionSqlGenerator
    {
        DatabaseQuery Query;
        Type EntityType;

        protected DatabaseCriterionSqlGenerator(DatabaseQuery query)
        {
            Query = query;
            EntityType = query.EntityType;
        }

        public string Generate(ICriterion criterion)
        {
            if (criterion == null) return "(1 = 1)";
            if (criterion is BinaryCriterion binary) return Generate(binary);
            else if (criterion is DirectDatabaseCriterion direct) return Generate(direct);
            else if (criterion.PropertyName.Contains(".")) return ToSubQuerySql(criterion);
            else return ToSqlOn(criterion, EntityType);
        }

        string Generate(BinaryCriterion criterion)
        {
            return $"({Generate(criterion.Left)} {criterion.Operator} {Generate(criterion.Right)})";
        }

        string Generate(DirectDatabaseCriterion criterion)
        {
            // Add the params:
            if (criterion.Parameters != null)
                foreach (var x in criterion.Parameters) Query.Parameters[x.Key] = x.Value;

            if (criterion.PropertyName.IsEmpty() || criterion.PropertyName == "N/A")
                return criterion.SqlCriteria;

            return criterion.SqlCriteria.Replace($"${{{{{criterion.PropertyName}}}}}",
              Query.Column(criterion.PropertyName));
        }

        string ToNestedSubQuerySql(ICriterion criterion, string[] parts)
        {
            var proc = new NestedCriteriaProcessor(EntityType, parts);

            var subCriterion = new Criterion(proc.Property.Name, criterion.FilterFunction, criterion.Value);

            var r = new StringBuilder();

            foreach (var sub in proc.Queries)
            {
                r.AppendLine("EXISTS (");
                r.AppendLine(sub.ToString());
                r.AppendLine("AND ");
            }

            r.Append(ToSqlOn(subCriterion, proc.Property.PropertyType, ToSafeId(proc.TableAlias)));

            foreach (var sub in proc.Queries) r.Append(")");

            return r.ToString();
        }

        protected abstract string ToSafeId(string id);

        string ToSubQuerySql(ICriterion criterion)
        {
            var parts = criterion.PropertyName.Split('.');
            return ToNestedSubQuerySql(criterion, parts);
        }

        string ToSqlOn(ICriterion criterion, Type type, string alias = null)
        {
            string column;

            var key = criterion.PropertyName;

            if (key.EndsWith("Id") && key.Length > 2)
            {
                var association = type.GetProperty(key.TrimEnd("Id"));

                if (association != null && !association.Defines<CalculatedAttribute>() &&
                    association.PropertyType.IsA<IEntity>())
                    key = key.TrimEnd("Id");
            }

            column = Query.Column(key, alias);

            var value = criterion.Value;
            var function = criterion.FilterFunction;

            if (value == null)
                return "{0} IS {1} NULL".FormatWith(column, "NOT".OnlyWhen(function != FilterFunction.Is));

            var valueData = value;
            if (function == FilterFunction.Contains || function == FilterFunction.NotContains) valueData = "%{0}%".FormatWith(value);
            else if (function == FilterFunction.BeginsWith) valueData = "{0}%".FormatWith(value);
            else if (function == FilterFunction.EndsWith) valueData = "%{0}".FormatWith(value);
            else if (function == FilterFunction.In)
            {
                if ((value as string) == "()") return "1 = 0 /*" + column + " IN ([empty])*/";
                else return column + " " + function.GetDatabaseOperator() + " " + value;
            }

            var parameterName = GetUniqueParameterName(column);

            Query.Parameters.Add(parameterName, valueData);

            var critera = $"{column} {function.GetDatabaseOperator()} @{parameterName}";
            var includeNulls = function == FilterFunction.IsNot;
            return includeNulls ? $"( {critera} OR {column} {FilterFunction.Null.GetDatabaseOperator()} )" : critera;
        }

        protected abstract string UnescapeId(string id);

        string GetUniqueParameterName(string column)
        {
            var result = column.Remove("[").Remove("]").Remove("`").Remove("\"").Replace(".", "_");

            if (Query.Parameters.ContainsKey(result))
            {
                for (var i = 2; ; i++)
                {
                    var name = result + "_" + i;
                    if (Query.Parameters.LacksKey(name)) return name;
                }
            }

            return result;
        }
    }
}