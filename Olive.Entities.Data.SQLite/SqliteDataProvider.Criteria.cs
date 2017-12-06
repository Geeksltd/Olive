using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Olive.Entities.Data.SQLite
{
    class SqliteCriterionGenerator
    {
        DatabaseQuery Query;
        Type EntityType;

        public SqliteCriterionGenerator(DatabaseQuery query)
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

            return criterion.SqlCriteria.Replace($"{{{{{criterion.PropertyName}}}}}",
              Query.Column(criterion.PropertyName));
        }

        string ToNestedSubQuerySql(ICriterion criterion, string[] parts)
        {
            var subQueries = new List<string>();
            var type = EntityType;

            subQueries.Add(Query.Provider.MapSubquery(parts[0] + ".*"));

            for (var i = 0; i < parts.Length - 2; i++)
            {
                type = type.GetProperty(parts[i]).PropertyType;

                var dataProvider = Database.Instance.GetProvider(type);
                var mapping = dataProvider.MapSubquery(parts[i + 1] + ".*");
                if (mapping.Lacks(" WHERE ")) continue;

                var subquerySplitted = mapping.Split(" WHERE ", StringSplitOptions.RemoveEmptyEntries);
                var whereClauseSplitted = subquerySplitted[1].Split('=');
                var tablePrefix = whereClauseSplitted.Last().Split('.')[0];

                var aliasName = tablePrefix.Trim(']').Trim(']').Split('_').ExceptLast().ToString("_") + "_";

                var recordIdentifier = whereClauseSplitted[1].Replace(tablePrefix, aliasName);

                mapping = subquerySplitted[0] + " WHERE " + whereClauseSplitted[0] + " = " + recordIdentifier;

                subQueries.Add(mapping);
            }

            var subCriterion = new Criterion(parts[parts.Length - 1], criterion.FilterFunction, criterion.Value);
            type = type.GetProperty(parts[parts.Length - 2]).PropertyType;
            var criteria = new StringBuilder();
            var parenthesis = "";
            for (var i = 0; i < subQueries.Count; i++)
            {
                criteria.AppendLine($" EXISTS ({subQueries[i].WithSuffix(" AND ")}");
                parenthesis += ")";
            }

            criteria.Append(ToSqlOn(subCriterion, type));
            criteria.Append(parenthesis);

            return criteria.ToString();
        }

        string ToSubQuerySql(ICriterion criterion)
        {
            var parts = criterion.PropertyName.Split('.');

            if (parts.Count() > 2) return ToNestedSubQuerySql(criterion, parts);

            var type = EntityType.GetProperty(parts[0])?.PropertyType;
            if (type == null) throw new Exception($"{EntityType.Name} does not have a public property named {parts[0]}.");

            var subquery = Query.Provider.MapSubquery(parts[0] + ".*");
            var subCriterion = new Criterion(parts[1], criterion.FilterFunction, criterion.Value);
            return "EXISTS ({0}{1})".FormatWith(subquery, ToSqlOn(subCriterion, type).WithPrefix(" AND "));
        }

        string ToSqlOn(ICriterion criterion, Type type)
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

            column = Query.Column(key);

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

        string GetUniqueParameterName(string column)
        {
            var result = column.Remove("[").Remove("]").Replace(".", "_");

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
