using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace Olive.Entities
{
    partial class Criterion
    {
        static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>> PropertiesCache =
            new ConcurrentDictionary<Type, ConcurrentDictionary<string, bool>>();

        protected virtual bool NeedsParameter(SqlConversionContext context) => true;

        protected virtual object GetValue(SqlConversionContext context)
        {
            if (FilterFunction == FilterFunction.Contains || FilterFunction == FilterFunction.NotContains) return $"%{Value}%";
            else if (FilterFunction == FilterFunction.BeginsWith) return $"{Value}%";
            else if (FilterFunction == FilterFunction.EndsWith) return $"%{Value}";

            return Value;
        }

        protected virtual string GetColumnName(SqlConversionContext context, string propertyName)
        {
            ValidateProperty(context.Type, propertyName);

            var key = propertyName;

            if (key.EndsWith("Id") && key.Length > 2)
            {
                var association = context.Type.GetProperty(key.TrimEnd("Id"));

                if (association != null && !association.Defines<CalculatedAttribute>() &&
                    association.PropertyType.IsA<IEntity>())
                    key = key.TrimEnd("Id");
            }

            return context.Query.Column(key, context.Alias);
        }

        void ValidateProperty(Type type, string propertyName)
        {
            var properties = PropertiesCache.GetOrAdd(type, t => new ConcurrentDictionary<string, bool>());

            var @throw = properties.GetOrAdd(propertyName, prop =>
            {
                var propertyInfo = type.GetProperty(prop);

                if (propertyInfo == null)
                    throw new Exception($"{type.FullName} has no public property named '{prop}'.");

                if (propertyInfo.Defines<EncryptedPropertyAttribute>()) return true;

                var temp = propertyInfo.GetCustomAttribute<CustomDataConverterAttribute>(inherit: false);
                if (temp != null && !(temp is EnumDataConverterAttribute)) return true;

                return false;
            });

            if (@throw)
                throw new Exception($"It is not possible to have criteria on '{propertyName}' as it is either encrypted or has custom data converter.");
        }

        public virtual string ToSql(SqlConversionContext context)
        {
            if (PropertyName.Contains(".")) return ToSubQuerySql(context);
            else return ToSqlOn(context);
        }

        string ToSubQuerySql(SqlConversionContext context)
        {
            var parts = PropertyName.Split('.');
            return ToNestedSubQuerySql(context, parts);
        }

        string ToNestedSubQuerySql(SqlConversionContext context, string[] parts)
        {
            if (context.Query.AliasPrefix.HasValue())
                throw new NotSupportedException("Conditions on associations is not supported when query is a sub query.");

            var proc = new NestedCriteriaProcessor(context.Type, parts);

            var subCriterion = new Criterion(proc.Property.Name, FilterFunction, Value);

            var r = new StringBuilder();

            foreach (var sub in proc.Queries)
            {
                r.AppendLine("EXISTS (");
                r.AppendLine(sub.ToString());
                r.AppendLine("AND ");
            }

            var newContext = new SqlConversionContext
            {
                Alias = context.ToSafeId(proc.TableAlias),
                Query = context.Query,
                ToSafeId = context.ToSafeId,
                Type = proc.Property.DeclaringType
            };

            r.Append(subCriterion.ToSqlOn(newContext));

            foreach (var sub in proc.Queries) r.Append(")");

            return r.ToString();
        }

        string GetUniqueParameterName(SqlConversionContext context, string column)
        {
            var result = column.Remove("[").Remove("]").Remove("`").Remove("\"").Replace(".", "_");

            if (context.Query.Parameters.ContainsKey(result))
            {
                for (var i = 2; ; i++)
                {
                    var name = result + "_" + i;
                    if (context.Query.Parameters.LacksKey(name)) return name;
                }
            }

            return result;
        }

        string ToSqlOn(SqlConversionContext context)
        {
            var column = GetColumnName(context, PropertyName);

            var value = GetValue(context);
            var function = FilterFunction;

            if (value == null)
                return "{0} IS {1} NULL".FormatWith(column, "NOT".OnlyWhen(function != FilterFunction.Is));

            if (function == FilterFunction.In)
            {
                if ((value as string) == "()") return "1 = 0 /*" + column + " IN ([empty])*/";
                else return column + " " + function.GetDatabaseOperator() + " " + value;
            }

            if (function == FilterFunction.NotIn)
            {
                if ((value as string) == "()") return "1 = 1 /*" + column + " Not IN ([empty])*/";
                else return column + " " + function.GetDatabaseOperator() + " " + value;
            }

            if (!NeedsParameter(context))
                return $"{column} {function.GetDatabaseOperator()} {value}";

            var parameterName = GetUniqueParameterName(context, column);

            context.Query.Parameters.Add(parameterName, value);

            var critera = $"{column} {function.GetDatabaseOperator()} @{parameterName}";
            var includeNulls = function == FilterFunction.IsNot;
            return includeNulls ? $"( {critera} OR {column} {FilterFunction.Null.GetDatabaseOperator()} )" : critera;
        }
    }
}