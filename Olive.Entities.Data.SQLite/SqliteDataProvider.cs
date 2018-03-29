using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Olive.Entities.Data.SQLite
{
    public abstract class SqliteDataProvider<TTargetEntity> : SqliteDataProvider where TTargetEntity : IEntity
    {
        public override Type EntityType => typeof(TTargetEntity);
    }
    public abstract partial class SqliteDataProvider : DataProvider<SqliteConnection, SqliteParameter>
    {
        public override IDataParameter GenerateParameter(KeyValuePair<string, object> data)
        {
            var value = data.Value;

            var result = new SqliteParameter { Value = value, ParameterName = data.Key.Remove(" ") };
            return result;
        }

        public override string GenerateSelectCommand(IDatabaseQuery iquery)
        {
            var query = (DatabaseQuery)iquery;

            if (query.PageSize.HasValue && query.OrderByParts.None())
                throw new ArgumentException("PageSize cannot be used without OrderBy.");

            var r = new StringBuilder("SELECT");

            r.Append(query.TakeTop.ToStringOrEmpty().WithPrefix(" TOP "));
            r.AppendLine($" {GetFields()} FROM {GetTables()}");
            r.AppendLine(GenerateWhere(query));
            r.AppendLine(GenerateSort(query).WithPrefix(" ORDER BY "));

            return r.ToString();
        }

        public override string GenerateWhere(DatabaseQuery query)
        {
            var r = new StringBuilder();

            if (SoftDeleteAttribute.RequiresSoftdeleteQuery(query.EntityType))
                query.Criteria.Add(new Criterion("IsMarkedSoftDeleted", false));

            r.Append($" WHERE { query.Column("ID")} IS NOT NULL");

            var whereGenerator = new SqliteCriterionGenerator(query);
            foreach (var c in query.Criteria)
                r.Append(whereGenerator.Generate(c).WithPrefix(" AND "));

            return r.ToString();
        }

        public override DirectDatabaseCriterion GetAssociationInclusionCriteria(IDatabaseQuery query, PropertyInfo association)
        {
            var whereClause = GenerateAssociationLoadingCriteria(query, association);
            return new DirectDatabaseCriterion(whereClause) { Parameters = query.Parameters };
        }

        string GenerateAssociationLoadingCriteria(IDatabaseQuery iquery, PropertyInfo association)
        {
            var query = (DatabaseQuery)iquery;

            if (query.PageSize.HasValue && query.OrderByParts.None())
                throw new ArgumentException("PageSize cannot be used without OrderBy.");

            var r = new StringBuilder();

            r.Append($"ID IN (");
            r.Append("SELECT ");
            r.Append(query.TakeTop.ToStringOrEmpty().WithPrefix(" TOP "));
            r.AppendLine($" {association.Name} FROM {GetTables()}");
            r.AppendLine(GenerateWhere(query));
            r.AppendLine((query.PageSize.HasValue ? GenerateSort(query) : string.Empty).WithPrefix(" ORDER BY "));
            r.Append(")");

            return r.ToString();
        }
    }
}