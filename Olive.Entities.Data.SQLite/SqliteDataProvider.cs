using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Olive.Entities.Data
{
    public abstract class SqliteDataProvider<TTargetEntity> : SqliteDataProvider where TTargetEntity : IEntity
    {
        protected SqliteDataProvider(ICache cache) : base(cache) { }
        public override Type EntityType => typeof(TTargetEntity);
    }

    public abstract partial class SqliteDataProvider : DataProvider<SqliteConnection, SqliteParameter>
    {
        protected SqliteDataProvider(ICache cache) : base(cache)
        {
        }

        public override IDataParameter GenerateParameter(KeyValuePair<string, object> data)
        {
            var value = data.Value;

            return new SqliteParameter { Value = value, ParameterName = data.Key.Remove(" ") };
        }

        public override string GenerateSelectCommand(IDatabaseQuery iquery, string fields)
        {
            var query = (DatabaseQuery)iquery;

            if (query.PageSize.HasValue && query.OrderByParts.None())
                throw new ArgumentException("PageSize cannot be used without OrderBy.");

            var r = new StringBuilder("SELECT");

            r.AppendLine($" {fields} FROM {GetTables()}");
            r.AppendLine(GenerateWhere(query));
            r.AppendLine(GenerateSort(query).WithPrefix(" ORDER BY "));
            r.AppendLine(query.TakeTop.ToStringOrEmpty().WithPrefix(" LIMIT "));

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

        protected override string SafeId(string objectName) => $"`{objectName}`";
    }
}